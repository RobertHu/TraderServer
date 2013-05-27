module Trader.Helper.Communication
open System
open System.Net.Security;
open Common
open Trader.Common
open log4net
open Serialization
open System.Collections.Concurrent


type Client(stream: SslStream,session: Int64, receiveCenter: IReceiveCenter) as this =
    static let logger = LogManager.GetLogger(typeof<Client>)
    let mutable buff = Array.zeroCreate 512
    let headerBuff = Array.zeroCreate Constants.HeadCount
    let stream = stream
    let session = ref session
    let receiver = receiveCenter
    let mutable readedHeadCount = 0
    let mutable readContentCount = 0
    
    [<VolatileField>]
    let  mutable isClosed = false
    let closeEvent = new Event<SenderClosedDelegate,SenderClosedEventArgs>()

    let agent = new Agent<byte[]>(fun inbox -> 
        async{
                try
                    while true do
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
                readedHeadCount <- count
                match count = Constants.HeadCount with
                |false -> 
                    while readedHeadCount <> Constants.HeadCount do
                        let! c = stream.AsyncRead(headerBuff,readedHeadCount,(Constants.HeadCount - readedHeadCount))
                        match c > 0 with
                        |false -> this.Close()
                        |true -> readedHeadCount <- readedHeadCount + c
                | _  -> ()
                let packetLength = Constants.GetPacketLength(headerBuff,0)
                let contentLength = packetLength - Constants.HeadCount
                match contentLength <= buff.Length with
                | false -> 
                    buff <- Array.zeroCreate contentLength
                | true -> ()
                readContentCount <- 0
                while readContentCount <> contentLength do
                    let! c = stream.AsyncRead(buff,readContentCount,(contentLength - readContentCount))
                    readContentCount <- readContentCount + c
                let packet = Array.zeroCreate packetLength
                System.Buffer.BlockCopy(headerBuff,0,packet,0,Constants.HeadCount)
                System.Buffer.BlockCopy(buff,0,packet,Constants.HeadCount,contentLength)
                receiver.Send(new ReceiveData(!session,packet))
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
             session :=  newsession
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

   

    



