module Trader.Helper.SendManager
open System
open System.Threading.Tasks
open Common
open Trader.Common
open Serialization
open log4net
let private Logger = LogManager.GetLogger("Trader.Helper.SendManager")
let SerializeMsg(msg: JobItem) =
    let mutable target : SerializedObject option = None
    match msg.Type with
    |JobType.Price ->
        target <- Some(new SerializedObject(msg.Price))
        
    |JobType.Transaction ->
        match msg.Content with
        |null ->
            match msg.ContentInByte with
            |null -> ()
            |_  ->
               target <- Some(new SerializedObject(msg.ContentInByte,msg.ClientInvokeID))

        |_ ->
            target <- Some(new SerializedObject(msg.Content,msg.ClientInvokeID))
            
    |_ -> ()

    match target with
    |Some(so) -> SerializeManager.Default.Serialize(so)
    |None -> null
        







