[<RequireQualifiedAccess>]
module FullCourtManager.SampleTeams

open FullCourtManager.Model
open System

let Bulls =
  { Name = "Bulls"
    Players = 
    [
        9, {Id=Guid.NewGuid (); JerseyNumber=9; Age=30<year>; Height=73<inch>; Weight=186<pound>; FirstName="Rajon"; LastName="Rondo"; Position=[PG]}
        3, {Id=Guid.NewGuid (); JerseyNumber=3; Age=34<year>; Height=76<inch>; Weight=220<pound>; FirstName="Dwyane"; LastName="Wade"; Position=[SG]}
        21, {Id=Guid.NewGuid (); JerseyNumber=21; Age=27<year>; Height=79<inch>; Weight=231<pound>; FirstName="Jimmy"; LastName="Butler"; Position=[SF]}
        22, {Id=Guid.NewGuid (); JerseyNumber=22; Age=31<year>; Height=81<inch>; Weight=236<pound>; FirstName="Taj"; LastName="Gibson"; Position=[PF]}
        8, {Id=Guid.NewGuid (); JerseyNumber=8; Age=28<year>; Height=84<inch>; Weight=255<pound>; FirstName="Robin"; LastName="Lopez"; Position=[C]}
    ] |> Map.ofList }

let Pacers =
  { Name = "Pacers"
    Players = 
    [
        9, {Id=Guid.NewGuid (); JerseyNumber=9; Age=30<year>; Height=73<inch>; Weight=186<pound>; FirstName="Rajon"; LastName="Rondo"; Position=[PG]}
        3, {Id=Guid.NewGuid (); JerseyNumber=3; Age=34<year>; Height=76<inch>; Weight=220<pound>; FirstName="Dwyane"; LastName="Wade"; Position=[SG]}
        21, {Id=Guid.NewGuid (); JerseyNumber=21; Age=27<year>; Height=79<inch>; Weight=231<pound>; FirstName="Jimmy"; LastName="Butler"; Position=[SF]}
        22, {Id=Guid.NewGuid (); JerseyNumber=22; Age=31<year>; Height=81<inch>; Weight=236<pound>; FirstName="Taj"; LastName="Gibson"; Position=[PF]}
        8, {Id=Guid.NewGuid (); JerseyNumber=8; Age=28<year>; Height=84<inch>; Weight=255<pound>; FirstName="Robin"; LastName="Lopez"; Position=[C]}
    ] |> Map.ofList }