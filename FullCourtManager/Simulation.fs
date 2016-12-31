module FullCourtManager.Simulation

open FullCourtManager.Model

type Configuration(logger:NLog.FSharp.Logger) =
    member __.Logger = logger
    member __.Random = System.Random ()

[<Literal>]
let QuarterLength = 720<sec>

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

[<AutoOpen>]
module UpdateState = 
    let getPossessionTeam state = 
        match state.Possession with 
        | Home -> state.HomeTeam 
        | Away -> state.AwayTeam

    let getDefendingTeam state = 
        match state.Possession with 
        | Home -> state.AwayTeam 
        | Away -> state.HomeTeam

    let togglePossession state = 
        match state.Possession with
        | Home -> Away
        | Away -> Home

    let updateTeamState f state =
        match state.Possession with
        | Home -> f state.HomeTeam
        | Away -> f state.AwayTeam

    let applyMade2ptResult applyChoice result state =
        match applyChoice, state.Possession with
        | Home, Home
        | Away, Away -> 
            let scoringTeam = getPossessionTeam state
            let sNumber = result.ShootingPlayer.JerseyNumber
            let sStats = scoringTeam.PlayerStats.[sNumber]

            let addStats = 
                Map.add sNumber {sStats with Attempt2pt = sStats.Attempt2pt + 1; Made2pt = sStats.Made2pt + 1}
                >> match result.AssistingPlayer with
                    | None -> id
                    | Some a -> 
                        let aStats = scoringTeam.PlayerStats.[a.JerseyNumber]
                        Map.add a.JerseyNumber {aStats with Assists = aStats.Assists + 1}

            {scoringTeam with Score = scoringTeam.Score + 2<point>; PlayerStats = addStats scoringTeam.PlayerStats}
        | Home, Away
        | Away, Home -> getDefendingTeam state

    let applyMade3ptResult applyChoice result state =
        match applyChoice, state.Possession with
        | Home, Home
        | Away, Away -> 
            let scoringTeam = getPossessionTeam state
            let sNumber = result.ShootingPlayer.JerseyNumber
            let sStats = scoringTeam.PlayerStats.[sNumber]

            let addStats =
                Map.add sNumber {sStats with Attempt3pt = sStats.Attempt3pt + 1; Made3pt = sStats.Made3pt + 1}
                >> match result.AssistingPlayer with
                    | None -> id
                    | Some a -> 
                        let aStats = scoringTeam.PlayerStats.[a.JerseyNumber]
                        Map.add a.JerseyNumber {aStats with Assists = aStats.Assists + 1}

            {scoringTeam with Score = scoringTeam.Score + 3<point>; PlayerStats = addStats scoringTeam.PlayerStats}
        | Home, Away
        | Away, Home -> getDefendingTeam state

    let applyMissed2ptResult applyChoice (result:MissedShotResult) state =
        match applyChoice, state.Possession with
        | Home, Home
        | Away, Away ->
            let shootingTeam = getPossessionTeam state
            let sNumber = result.ShootingPlayer.JerseyNumber
            let sStats = shootingTeam.PlayerStats.[sNumber]
            
            let addStats =
                Map.add sNumber {sStats with Attempt2pt = sStats.Attempt2pt + 1;}
                >> match result.OffReboundingPlayer with 
                    | None -> id
                    | Some r -> 
                        let rStats = shootingTeam.PlayerStats.[r.JerseyNumber]
                        Map.add r.JerseyNumber {rStats with Rebounds = rStats.Rebounds + 1}

            {shootingTeam with PlayerStats = addStats shootingTeam.PlayerStats}
        | Home, Away
        | Away, Home ->
            let defendingTeam = getDefendingTeam state
            let addStats =
                match result.DefReboundingPlayer with
                | None -> id
                | Some r ->
                    let rStats = defendingTeam.PlayerStats.[r.JerseyNumber]
                    Map.add r.JerseyNumber {rStats with Rebounds = rStats.Rebounds + 1}
        
            {defendingTeam with PlayerStats = addStats defendingTeam.PlayerStats}

    let applyMissed3ptResult applyChoice (result:MissedShotResult) state =
        match applyChoice, state.Possession with
        | Home, Home
        | Away, Away ->
            let shootingTeam = getPossessionTeam state
            let sNumber = result.ShootingPlayer.JerseyNumber
            let sStats = shootingTeam.PlayerStats.[sNumber]
        
            let addStats = 
                Map.add sNumber {sStats with Attempt3pt = sStats.Attempt3pt + 1;}
                >> match result.OffReboundingPlayer with 
                    | None -> id
                    | Some r -> 
                        let rStats = shootingTeam.PlayerStats.[r.JerseyNumber]
                        Map.add r.JerseyNumber {rStats with Rebounds = rStats.Rebounds + 1}

            {shootingTeam with PlayerStats = addStats shootingTeam.PlayerStats}
        | Home, Away
        | Away, Home ->
            let defendingTeam = getDefendingTeam state
            let addStats =
                match result.DefReboundingPlayer with
                | None -> id
                | Some r ->
                    let rStats = defendingTeam.PlayerStats.[r.JerseyNumber]
                    Map.add r.JerseyNumber {rStats with Rebounds = rStats.Rebounds + 1}
        
            {defendingTeam with PlayerStats = addStats defendingTeam.PlayerStats}

    let applyPlayResult state playResult =
        let state' = { state with PlayByPlay = [playResult] |> List.append state.PlayByPlay }
        match playResult with
        | Made2pt r ->
            { state' with
                SecondsRemainingInQuarter = state.SecondsRemainingInQuarter - r.TimeElapsed 
                HomeTeam = state' |> applyMade2ptResult Home r
                AwayTeam = state' |> applyMade2ptResult Away r
                Possession = togglePossession state' }
        | Made3pt r ->
            { state' with
                SecondsRemainingInQuarter = state.SecondsRemainingInQuarter - r.TimeElapsed 
                HomeTeam = state' |> applyMade2ptResult Home r
                AwayTeam = state' |> applyMade2ptResult Away r
                Possession = togglePossession state' }
        | Missed2pt r ->
            {state' with 
                SecondsRemainingInQuarter = state.SecondsRemainingInQuarter - r.TimeElapsed 
                HomeTeam = state' |> applyMissed2ptResult Home r
                AwayTeam = state' |> applyMissed2ptResult Away r
                Possession = 
                    match r.DefReboundingPlayer with 
                    | Some p -> togglePossession state' 
                    | None -> state'.Possession}
        | Missed3pt r ->
            {state' with 
                SecondsRemainingInQuarter = state.SecondsRemainingInQuarter - r.TimeElapsed 
                HomeTeam = state' |> applyMissed3ptResult Home r
                AwayTeam = state' |> applyMissed3ptResult Away r
                Possession = 
                    match r.DefReboundingPlayer with 
                    | Some p -> togglePossession state' 
                    | None -> state'.Possession}
        | ShotClockViolation -> 
            {state' with
                SecondsRemainingInQuarter = state.SecondsRemainingInQuarter - 24<sec> 
                Possession = togglePossession state'}
        | _ -> state'

    let advanceQuarter state =
        if state.SecondsRemainingInQuarter > 0<sec>
        then state
        else
            match state.CurrentQuarter with
            | First -> {state with CurrentQuarter = Second; SecondsRemainingInQuarter = QuarterLength}
            | Second -> {state with CurrentQuarter = Third; SecondsRemainingInQuarter = QuarterLength}
            | Third -> {state with CurrentQuarter = Fourth; SecondsRemainingInQuarter = QuarterLength}
            | Fourth | Overtime ->
                match state with
                | TieGame -> {state with CurrentQuarter = Overtime; SecondsRemainingInQuarter = QuarterLength}
                | HomeLeads | AwayLeads -> state
    
let simulatePlay state =
    ShotClockViolation

let initializeTeamState team =
    let playerStats = team.Players |> Map.map (fun n p -> InitialPlayerState)
    {Score = 0<point>; TeamFouls = 0; TeamInfo = team; PlayerStats = playerStats}

let initializeGameState homeTeam awayTeam = 
    { CurrentQuarter = First
      SecondsRemainingInQuarter = QuarterLength
      Possession = Home
      HomeTeam = homeTeam |> initializeTeamState
      AwayTeam = awayTeam |> initializeTeamState
      PlayByPlay = List.empty }

let logPlayResult (logger:NLog.FSharp.Logger) (result:PlayResult) =
    match result with
    | ShotClockViolation -> logger.Info "%s" "Shot clock violation!"
    result

let simulateNextPlay (config:Configuration) state =
    simulatePlay state |> logPlayResult config.Logger |> applyPlayResult state |> advanceQuarter

let buildSimulator config : GameSimulator =
    let simPlay = simulateNextPlay config
    let rec loop state =
        async {
            match state with
            | CompleteGame -> return state
            | IncompleteGame -> return! simPlay state |> loop
        }

    loop