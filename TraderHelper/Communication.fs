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
    let stream = stream
    let session = ref session
    let receiver = receiveCenter
    let mutable readedHeadCount = 0
    let mutable readContentCount = 0
    let mutable bufferIndex = -1
    
    [<VolatileField>]
    let  mutable isClosed = false
    let closeEvent = new Event<SenderClosedDelegate,SenderClosedEventArgs>()

    let agent = new Agent<byte[]>(fun inbox -> 
        async{
                try
                    while true do
                        let! msg= inbox.Receive()
                        match msg.Length > BufferManager.BUFFER_SIZE / 2 with
                        |true -> do! stream.AsyncWrite(msg)
                        |false -> 
                            let offset = bufferIndex + BufferManager.BUFFER_SIZE / 2
                            System.Buffer.BlockCopy(msg,0,BufferManager.Default.Buffer,offset,msg.Length)
                            do! stream.AsyncWrite(BufferManager.Default.Buffer,offset,msg.Length)
                with
                    |x ->
                        this.Close()
                }
        )


    let read() = async{
        while not isClosed do
            try
                let! count = stream.AsyncRead(BufferManager.Default.Buffer,bufferIndex,Constants.HeadCount)
                readedHeadCount <- count
                match count = Constants.HeadCount with
                |false -> 
                    while readedHeadCount <> Constants.HeadCount do
                        let! c = stream.AsyncRead(BufferManager.Default.Buffer,readedHeadCount + bufferIndex,(Constants.HeadCount - readedHeadCount))
                        match c > 0 with
                        |false -> this.Close()
                        |true -> readedHeadCount <- readedHeadCount + c
                | _  -> ()
                let packetLength = Constants.GetPacketLength(BufferManager.Default.Buffer,bufferIndex)
                let contentLength = packetLength - Constants.HeadCount
                readContentCount <- 0
                while readContentCount <> contentLength do
                    let offset = bufferIndex + readContentCount + Constants.HeadCount
                    let! c = stream.AsyncRead(BufferManager.Default.Buffer,offset,(contentLength - readContentCount))
                    readContentCount <- readContentCount + c
                let packet = Array.zeroCreate packetLength
                System.Buffer.BlockCopy(BufferManager.Default.Buffer,bufferIndex,packet,0,Constants.HeadCount)
                let contentOffset = bufferIndex + Constants.HeadCount
                System.Buffer.BlockCopy(BufferManager.Default.Buffer,contentOffset,packet,Constants.HeadCount,contentLength)
                let receiveData = ReceiveDataPool.Default.Pop()
                if receiveData = null then receiver.Send(new ReceiveData(!session,packet))
                else
                    receiveData.Session <- !session
                    receiveData.Data <- packet 
                    receiver.Send(receiveData)
            with
            |x -> 
                this.Close()
        }
    do
        Async.Start(read())
        agent.Start()

    interface ICommunicationAgent with
        member this.Send(msg: byte[])  = if not isClosed then agent.Post(msg)

        member this.UpdateSession(newsession) = session :=  newsession
        [<CLIEvent>]
        member this.Closed = closeEvent.Publish
        member this.BufferIndex
            with get() = bufferIndex and set(value) = bufferIndex <- value


    member this.IsClosed 
        with get() = isClosed and set(value) = isClosed <- value
    
    


    member private this.Close() =
        match this.IsClosed with
        |true -> ()
        | _ ->
            this.IsClosed <- true
            stream.Close()
            closeEvent.Trigger(new obj(), new SenderClosedEventArgs(!session))
            BufferManager.Default.FreeBuffer(bufferIndex)

   

    



