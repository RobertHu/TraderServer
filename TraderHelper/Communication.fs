module Trader.Helper.Communication
open System
open System.Net.Security;
open Common
open Trader.Common
open log4net
open Serialization

type Client(stream: SslStream,session: Guid) as this =
    let logger = LogManager.GetLogger(typeof<Client>)
    let buff = Array.zeroCreate 4096
    let headerBuff = Array.zeroCreate Constants.HeadCount
    let stream = stream
    let session = ref session
    let syncLock = new obj()
    [<VolatileField>]
    let  mutable isClosed = false
    let event = new Event<DataArrivedDelegate,DataArrivedEventArgs>()
    let closeEvent = new Event<SenderClosedDelegate,SenderClosedEventArgs>()

    let packetArrivedHandler (packet : byte[]) =
            try
               event.Trigger(this, new DataArrivedEventArgs(packet,!session))
            with
            |x -> logger.Error(x)

    let agent = new Agent<byte[]>(fun inbox -> 
            let rec loop() = async{
                    try
                        let! msg= inbox.Receive() 
                        do! stream.AsyncWrite(msg)
                        return! loop()
                    with
                    |x ->
                        logger.Error(x)
                        this.Close()
                }
            loop()

        )

    let rec read() = async{
            try
                let! count = stream.AsyncRead(headerBuff)

                let rec readHead readCount =  
                    async{
                        match readCount = Constants.HeadCount with
                        | false ->
                            let! c = stream.AsyncRead(headerBuff,readCount,(Constants.HeadCount - readCount))
                            return! readHead (c + readCount)
                        | true -> ()
                            
                    }

                match count = Constants.HeadCount with
                |false -> do! readHead count
                | _  -> ()

                let packetLength = Constants.GetPacketLength(headerBuff,0)
                let contentLength = packetLength - Constants.HeadCount
                let rec readFull (myBuf: byte[]) (readCount: int) =
                   async{
                        match readCount = contentLength with
                        |false ->
                            let! c = stream.AsyncRead(myBuf,readCount,(contentLength - readCount))
                            return! readFull myBuf (readCount + c)
                        |true ->
                            let packet = Array.zeroCreate packetLength
                            System.Array.Copy(headerBuff,packet,Constants.HeadCount)
                            System.Array.Copy(myBuf,0,packet,Constants.HeadCount,contentLength)
                            packetArrivedHandler(packet)
                   }

                match contentLength <= buff.Length with
                | false -> 
                    let tempBuf = Array.zeroCreate contentLength
                    do! readFull tempBuf 0
                | true -> 
                    do! readFull buff 0

                return! read()
            with
            |x -> 
                this.Close()
        }
    do
        Async.Start(read())
        agent.Start()


    interface ICommunicationAgent with
        member this.Send(msg: byte[])  = 
            match isClosed with
            |true -> ()
            |false -> agent.Post(msg)
        member this.UpdateSession(newsession) =
            lock syncLock (fun () -> session :=  newsession)
        [<CLIEvent>]
        member this.DataArrived = event.Publish
        [<CLIEvent>]
        member this.Closed = closeEvent.Publish


    member this.IsClosed 
            with get() =
                 isClosed 
         
            and set(value) = 
                isClosed <- value
    

    member private this.Close() =
        match this.IsClosed with
        |true -> ()
        | _ ->
            this.IsClosed <- true
            stream.Close()
            closeEvent.Trigger(new obj(), new SenderClosedEventArgs(!session))

   

    



