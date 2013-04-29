module Trader.Helper.Common
open System
open Trader.Common


let internal QUEUE_MAX_COUNT = 200

type internal Agent<'a> = MailboxProcessor<'a>

type DataArrivedEventArgs(data: byte[], session: Guid) =
    inherit EventArgs()
    member this.Data = data
    member this.Session = session
    
type DataArrivedDelegate =  delegate of obj * DataArrivedEventArgs -> unit

type ResponseEventArgs(job: JobItem) =
    inherit EventArgs()
    member this.Job = job
type SendResponseDelegate = delegate of obj * ResponseEventArgs -> unit

type SessionTimeoutEventArgs(session: string) =
    inherit EventArgs()
    member this.Session = session

type SessionTimeoutDelegate = delegate of obj * SessionTimeoutEventArgs -> unit

type SenderClosedEventArgs(session: Guid) =
    inherit EventArgs()
    member this.Session = session


type SenderClosedDelegate = delegate of obj * SenderClosedEventArgs -> unit

[<AllowNullLiteralAttribute>]
type IReceiveAgent =
    abstract Send : Guid * byte[] -> unit
    [<CLIEvent>]
    abstract ResponseSent : IEvent<SendResponseDelegate, ResponseEventArgs>

[<AllowNullLiteralAttribute>]
type ICommunicationAgent =
    abstract Send :byte[] -> unit
    abstract UpdateSession: Guid -> unit
    [<CLIEvent>]
    abstract DataArrived: IEvent<DataArrivedDelegate,DataArrivedEventArgs>
    [<CLIEvent>]
    abstract Closed: IEvent<SenderClosedDelegate, SenderClosedEventArgs>
    
type ClientDisconnectedEventArgs(session: Guid, client: ICommunicationAgent, receiveClient: IReceiveAgent) =
    inherit EventArgs()
    member this.Session = session
    member this.Client = client
    member this.ReceiveClient = receiveClient

type ClientDisconnectedDelegate = delegate of obj * ClientDisconnectedEventArgs -> unit
 

