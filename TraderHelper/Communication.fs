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
            async{
                while true do
                    try
                        let! msg= inbox.Receive() 
                        do! stream.AsyncWrite(msg)
                    with
                    |x ->
                        //logger.Error(x)
                        this.Close()
                }

        )

    let read() = async{
        while not isClosed do
            try
                let! count = stream.AsyncRead(headerBuff)
                let readedHeadCount = ref count
                match count = Constants.HeadCount with
                |false -> 
                    while !readedHeadCount <> Constants.HeadCount do
                        let! c = stream.AsyncRead(headerBuff,!readedHeadCount,(Constants.HeadCount - !readedHeadCount))
                        readedHeadCount := !readedHeadCount + c
                | _  -> ()
                let packetLength = Constants.GetPacketLength(headerBuff,0)
                let contentLength = packetLength - Constants.HeadCount
                let tempBuf= ref null
                match contentLength <= buff.Length with
                | false -> 
                    tempBuf := Array.zeroCreate contentLength
                    
                | true -> tempBuf := buff
                let readContentCount = ref 0    
                while !readContentCount <> contentLength do
                    let! c = stream.AsyncRead(!tempBuf,!readContentCount,(contentLength - !readContentCount))
                    readContentCount := !readContentCount + c
                let packet = Array.zeroCreate packetLength
                System.Array.Copy(headerBuff,packet,Constants.HeadCount)
                System.Array.Copy(!tempBuf,0,packet,Constants.HeadCount,contentLength)
                packetArrivedHandler(packet)
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

   

    



