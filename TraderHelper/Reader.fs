module Trader.Helper.Reader
open System
open System.Net.Security
open Trader.Common
open Serialization

type ClientReader(stream: SslStream,session: Int64, receiveCenter: IReceiveCenter) as this =
    let buff = Array.zeroCreate 512
    let headerBuff = Array.zeroCreate Constants.HeadCount
    let stream = stream
    let session = ref session
    let receiver = receiveCenter

    [<VolatileField>]
    let  mutable isClosed = false

    let read() = async{
        let readedHeadCount = ref 0
        let readContentCount = ref 0 
        let tempBuf= ref null
        while not isClosed do
            try
                let! count = stream.AsyncRead(headerBuff)
                readedHeadCount := count
                match count = Constants.HeadCount with
                |false -> 
                    while !readedHeadCount <> Constants.HeadCount do
                        let! c = stream.AsyncRead(headerBuff,!readedHeadCount,(Constants.HeadCount - !readedHeadCount))
                        match c > 0 with
                        |false -> this.Close()
                        |true -> readedHeadCount := !readedHeadCount + c
                | _  -> ()
                let packetLength = Constants.GetPacketLength(headerBuff,0)
                let contentLength = packetLength - Constants.HeadCount
                match contentLength <= buff.Length with
                | false -> 
                    tempBuf := Array.zeroCreate contentLength
                | true -> tempBuf := buff
                readContentCount := 0
                while !readContentCount <> contentLength do
                    let! c = stream.AsyncRead(!tempBuf,!readContentCount,(contentLength - !readContentCount))
                    readContentCount := !readContentCount + c
                let packet = Array.zeroCreate packetLength
                System.Buffer.BlockCopy(headerBuff,0,packet,0,Constants.HeadCount)
                System.Buffer.BlockCopy(!tempBuf,0,packet,Constants.HeadCount,contentLength)
                receiver.Send(new Trader.Common.ReceiveData(!session,packet))
            with
            |x -> 
                this.Close()
        }

    member this.IsClosed 
        with get() = isClosed 
        and set(value) = isClosed <- value
    
    member this.Start() = Async.Start(read())

    member private this.Close() =
        match this.IsClosed with
        |true -> ()
        | _ ->
            this.IsClosed <- true
            stream.Close()
    member this.GetSession() = !session
               
    member this.UpdateSession(newsession) =
             session :=  newsession
               