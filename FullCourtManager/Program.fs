
open FullCourtManager

[<EntryPoint>]
let main argv = 
    
    let logger = NLog.LogManager.GetLogger "FullCourtManager.Program" |> NLog.FSharp.Logger
    let config = logger |> Simulation.Configuration

    let bulls = SampleTeams.Bulls
    let pacers = SampleTeams.Pacers

    let state = Simulation.initializeGameState bulls pacers
    let simulator = Simulation.buildSimulator config
    let finalState = simulator state |> Async.RunSynchronously
    0
