module Trader.Helper.ReceiveManager
open System
open Common
open Serialization
open SendManager
open Trader.Common
open log4net

let parseData session data =
    let result = PacketParser.Parse(data)
    match result with
    |null -> None
    |_ -> 
        match String.IsNullOrEmpty(result.Session) with
        |true ->
            result.Session <- session
        | false -> 
            result.CurrentSession <- session
        Some(result) 

type processRequestDelegate = delegate of SerializedObject -> JobItem

type ReceiveAgent(f: processRequestDelegate) =
    let logger = LogManager.GetLogger(typeof<ReceiveAgent>)
    let processor = f
    let event = new Event<SendResponseDelegate, ResponseEventArgs>()
    let agent = new Agent<Guid * byte[]>(fun inbox ->
        async{
            while true do
                let! session,data = inbox.Receive()
                match parseData (session.ToString()) data with
                |None -> ()
                |Some request ->
                    try
                        let result = f.Invoke(request)
                        event.Trigger(new obj(),  new ResponseEventArgs(result))
                    with
                    | x -> ()
            }
        )

    do
        agent.Start()

    interface IReceiveAgent with
        member this.Send(session,data) = agent.Post(session,data)
        [<CLIEventAttribute>]
        member this.ResponseSent = event.Publish







