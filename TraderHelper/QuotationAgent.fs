module Trader.Helper.QuotationAgent
open Common
open System
open System.Collections.Generic
open System.Threading.Tasks
open log4net
type SendCommandDelegate = delegate of obj * Guid * ICommunicationAgent  -> unit

type QuotationMsg =
    |Add of Guid * ICommunicationAgent
    |Remove of Guid
    |Send  of obj * SendCommandDelegate

type Quotation private() =
    let logger = LogManager.GetLogger(typeof<Quotation>)
    static let instance = new Quotation()
    let dict = new Dictionary<Guid,ICommunicationAgent>()
    let agent = new Agent<QuotationMsg>(fun inbox ->
            async{
                while true do
                    let! msg = inbox.Receive()
                    match msg with
                    |Add(session, client) ->
                        match dict.TryGetValue(session) with
                        |(true, _ ) -> ()
                        | _ -> dict.Add(session,client)

                    |Remove(session) ->
                        match dict.ContainsKey(session) with
                        |true ->
                            dict.Remove(session) |> ignore
                            logger.Info("remove session")
                        |_ -> ()
                    |Send(command,f) ->
                        try
                            match dict.Count with
                            |0 ->
                                logger.Info("no client")
                            |_ -> 
                                 Parallel.ForEach(dict,fun (p: KeyValuePair<Guid,ICommunicationAgent>) -> 
                                    try
                                        f.Invoke(command,p.Key, p.Value)
                                    with
                                    |x -> ()
                                  ) |> ignore
                        with
                        | :? System.AggregateException as ae ->
                            ae.InnerExceptions
                            |>Seq.iter (fun ex -> printfn "%s" ex.Message)
                            
                        | _ -> ()
                }
        )

    do
        agent.Start()

    static member Default = instance
    member this.Send(c,f) = agent.Post(Send(c,f))

    member this.Add(session, client) = agent.Post(Add(session,client))

    member this.Remove(session) = agent.Post(Remove(session))