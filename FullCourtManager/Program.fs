
[<Measure>] type sec
[<Measure>] type min
[<Measure>] type year
[<Measure>] type point
[<Measure>] type inch
[<Measure>] type pound

type Player = {
    Id: System.Guid
    JerseyNumber: int
    Age: int<year>
    Height: int<inch>
    Weight: int<pound>
    FirstName: string
    LastName: string
}

type Team = {
    Name: string
    Players: Map<int, Player>
}

type GameQuarter = First | Second | Third | Fourth | Overtime

type Shot = Dunk | Layup | Floater | JumpShot | PostUp

type MissedShotResult = {
    ShootingPlayer: Player
    BlockingPlayer: Player option
    OffReboundingPlayer: Player option
    DefReboundingPlayer: Player option
    ShotType: Shot
    TimeElapsed: int<sec>
}

type MadeShotResult = {
    ShootingPlayer: Player
    AssistingPlayer: Player option
    ShotType: Shot
    TimeElapsed: int<sec>
}

type FreeThrowResult = {
    Awarded: int
    Made: int
}

type FoulResult = {
    FoulingPlayer: Player
    FouledPlayer: Player option
    Made2pt: bool
    Made3pt: bool
    FreeThrows: FreeThrowResult option
    TimeElapsed: int<sec>
}

type StealResult = {
    StealingPlayer: Player
    StolenPlayer: Player
}

type PlayResult =
    | Missed2pt of MissedShotResult
    | Made2pt of MadeShotResult
    | Missed3pt of MissedShotResult
    | Made3pt of MadeShotResult
    | CommonFoul of FoulResult
    | ShootingFoul of FoulResult
    | Steal of StealResult
    | ShotClockViolation

type PlayerState = {
    Points: int<point>
    Fouls: int
    MinutesPlayed: int<min>
    Blocks: int
    Steals: int
    Assists: int
    Turnovers: int
}

type TeamState = {
    Score: int<point>
    TeamFouls: int
    TeamInfo: Team
    PlayerStats: Map<int, PlayerState>
}

type TeamWithPossesion = Home | Away

type GameState = {
    CurrentQuarter: GameQuarter
    SecondsRemainingInQuarter: int<sec>
    Poession: TeamWithPossesion
    HomeTeam: TeamState
    AwayTeam: TeamState
    PlayByPlay: PlayResult[]
}

type PlaySimulation = GameState -> Async<PlayResult>

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0
