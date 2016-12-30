module FullCourtManager.Model

[<Measure>] type sec
[<Measure>] type year
[<Measure>] type point
[<Measure>] type inch
[<Measure>] type pound

type Position = PG | SG | SF | PF | C

type Player = {
    Id: System.Guid
    JerseyNumber: int
    Position: Position list
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
    TimeElapsed: int<sec>
}

type PlayResult =
    | Missed2pt of MissedShotResult
    | Made2pt of MadeShotResult
    | Missed3pt of MissedShotResult
    | Made3pt of MadeShotResult
    | CommonFoul of FoulResult
    | ShootingFoul of FoulResult
    | OffensiveFoul of FoulResult
    | Steal of StealResult
    | ShotClockViolation

type PlayerState = {
    Points: int<point>
    Fouls: int
    SecondsPlayed: int<sec>
    Blocks: int
    Steals: int
    Assists: int
    Turnovers: int
    InGame: bool
    Attempt2pt: int
    Made2pt: int
    Attempt3pt: int
    Made3pt: int
}

let InitialPlayerState = {
    Points = 0<point>
    Fouls = 0
    SecondsPlayed = 0<sec>
    Blocks = 0
    Steals = 0
    Assists = 0
    Turnovers = 0
    InGame = false
    Attempt2pt = 0
    Made2pt = 0
    Attempt3pt = 0
    Made3pt = 0
}

type TeamState = {
    Score: int<point>
    TeamFouls: int
    TeamInfo: Team
    PlayerStats: Map<int, PlayerState>
}

type TeamChoice = Home | Away

type GameState = {
    CurrentQuarter: GameQuarter
    SecondsRemainingInQuarter: int<sec>
    Possession: TeamChoice
    HomeTeam: TeamState
    AwayTeam: TeamState
    PlayByPlay: PlayResult list
}

// Given the game state, simulate the next play
type PlaySimulator = GameState -> Async<PlayResult>

// Given the initial game state, simulate the game result
type GameSimulator = GameState -> Async<GameState>