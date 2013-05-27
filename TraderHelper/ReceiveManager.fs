module Trader.Helper.ReceiveManager
open System
open Common
open Serialization
open Trader.Common
open log4net

let parseData (data: ReceiveData) =
    let result = PacketParser.Parse(data.Data)
    match result with
    |null -> null
    |_ -> 
        match result.Session <> SessionMapping.INVALID_VALUE with
        |false ->
            result.Session <- data.Session
        |true -> 
            result.CurrentSession <- data.Session
        result 

type processRequestDelegate = delegate of SerializedObject -> unit

type ReceiveAgent(f: processRequestDelegate) =
    static let logger = LogManager.GetLogger(typeof<ReceiveAgent>)
    let processor = f
    let agent = new Agent<ReceiveData>(fun inbox ->
        async{
            while true do
                let! data = inbox.Receive()
                match parseData data with
                |null -> ()
                |request ->
                    try
                        f.Invoke(request)
                    with
                    | x -> ()
            }
        )

    do
        agent.Start()

    interface IReceiveAgent with
        member this.Send(data) = agent.Post(data)







