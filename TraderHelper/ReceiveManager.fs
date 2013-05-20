module Trader.Helper.ReceiveManager
open System
open Common
open Serialization
open Trader.Common
open log4net

let parseData (session: System.Nullable<Int64>) data =
    let result = PacketParser.Parse(data)
    match result with
    |null -> None
    |_ -> 
        match result.Session.HasValue with
        |false ->
            result.Session <- session
        |true -> 
            result.CurrentSession <- session
        Some(result) 

type processRequestDelegate = delegate of SerializedObject -> unit

type ReceiveAgent(f: processRequestDelegate) =
    static let logger = LogManager.GetLogger(typeof<ReceiveAgent>)
    let processor = f
    let agent = new Agent<System.Nullable<Int64> * byte[]>(fun inbox ->
        async{
            while true do
                let! session,data = inbox.Receive()
                match parseData session data with
                |None -> ()
                |Some request ->
                    try
                        f.Invoke(request)
                    with
                    | x -> ()
            }
        )

    do
        agent.Start()

    interface IReceiveAgent with
        member this.Send(session,data) = agent.Post(session,data)







