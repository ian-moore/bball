
open FullCourtManager

[<EntryPoint>]
let main argv = 
    let bulls = SampleTeams.Bulls
    let pacers = SampleTeams.Pacers

    let state = Simulation.initializeGameState bulls pacers
    let random = System.Random ()
    let simulator = Simulation.buildSimulator random
    let finalState = simulator state |> Async.RunSynchronously
    0
