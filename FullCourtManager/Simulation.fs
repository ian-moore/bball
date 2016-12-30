module FullCourtManager.Simulation

open FullCourtManager.Model

let initializeTeamState team =
    let playerStats = team.Players |> Map.map (fun n p -> InitialPlayerState)
    { Score = 0<point>
      TeamFouls = 0
      TeamInfo = team 
      PlayerStats = playerStats }

[<Literal>]
let QuarterLength = 720<sec>

let initializeGameState homeTeam awayTeam = 
    { CurrentQuarter = First
      SecondsRemainingInQuarter = QuarterLength
      Possession = Home
      HomeTeam = homeTeam |> initializeTeamState
      AwayTeam = awayTeam |> initializeTeamState
      PlayByPlay = List.empty }

let (|CompleteGame|IncompleteGame|) (s:GameState) =
    if s.SecondsRemainingInQuarter > 0<sec>
    then IncompleteGame
    else match s.CurrentQuarter with
         | Fourth | Overtime -> CompleteGame
         | _ -> IncompleteGame

let (|TieGame|HomeLeads|AwayLeads|) (s:GameState) =
    match s.HomeTeam.Score - s.AwayTeam.Score with
    | i when i = 0<point> -> TieGame
    | i when i > 0<point> -> HomeLeads
    | i when i < 0<point> -> AwayLeads

let togglePossession state = 
    match state.Possession with
    | Home -> { state with Possession = Away }
    | Away -> { state with Possession = Home }

let updateTeamState f state =
    match state.Possession with
    | Home -> { state with HomeTeam = f state.HomeTeam }
    | Away -> { state with AwayTeam = f state.AwayTeam }

let applyPlayResult state playResult =
    let state' = { state with PlayByPlay = List.append state.PlayByPlay [playResult] }
    match playResult with
    | Made2pt r ->
        { state' with
            SecondsRemainingInQuarter = state.SecondsRemainingInQuarter - r.TimeElapsed }
        |> updateTeamState (fun s ->
            let n = r.ShootingPlayer.JerseyNumber
            let p = s.PlayerStats.[n]
            s.PlayerStats.Add(n, { p with Attempt2pt = p.Attempt2pt + 1; Made2pt = p.Made2pt + 1}) |> ignore
            { s with Score = s.Score + 2<point> })
        |> togglePossession
    | Missed2pt r ->
        { state' with 
            SecondsRemainingInQuarter = state.SecondsRemainingInQuarter - r.TimeElapsed }
        |> updateTeamState (fun s -> 
            let n = r.ShootingPlayer.JerseyNumber
            let p = s.PlayerStats.[n]
            s.PlayerStats.Add(n, { p with Attempt2pt = p.Attempt2pt + 1 }) |> ignore
            s)
        |> (fun s -> match r.DefReboundingPlayer with Some p -> togglePossession s | None -> s)
    | ShotClockViolation -> 
        { state' with
            SecondsRemainingInQuarter = state.SecondsRemainingInQuarter - 24<sec> }
        |> togglePossession
    | _ -> state'

let advanceQuarter state =
    if state.SecondsRemainingInQuarter > 0<sec>
    then state
    else
        match state.CurrentQuarter with
        | First -> { state with CurrentQuarter = Second; SecondsRemainingInQuarter = QuarterLength }
        | Second -> { state with CurrentQuarter = Third; SecondsRemainingInQuarter = QuarterLength }
        | Third -> { state with CurrentQuarter = Fourth; SecondsRemainingInQuarter = QuarterLength }
        | Fourth | Overtime ->
            match state with
            | TieGame -> { state with CurrentQuarter = Overtime; SecondsRemainingInQuarter = QuarterLength }
            | HomeLeads | AwayLeads -> state
    
let simulatePlay state =
    ShotClockViolation

let simulateNextPlay config state =
    simulatePlay state |> applyPlayResult state |> advanceQuarter

let buildSimulator config : GameSimulator =
    let simPlay = simulateNextPlay config
    let rec loop state =
        async {
            match state with
            | CompleteGame -> return state
            | IncompleteGame -> return! simPlay state |> loop
        }

    loop