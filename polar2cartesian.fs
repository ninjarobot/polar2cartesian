open System

type Polar = {
    Radius: float
    Theta: float
}

type Cartesian ={
    X: float
    Y: float
}

type PolarMessage = {
    ReplyChannel: AsyncReplyChannel<Cartesian>
    Polar: Polar
}

let prompt = 
    match Environment.OSVersion.Platform with 
    | PlatformID.Unix | PlatformID.MacOSX -> // Linux / OS X
        "Ctrl+C"
    | _ ->
        "Ctrl+C, Enter"
    |> sprintf "Enter a radius and an angle (in degrees) or %s to quit."

let polarToCartesianSolver = 
    MailboxProcessor<PolarMessage>.Start(fun inbox ->
        let rec msgLoop () = async {
            let! msg = inbox.Receive ()
            let polar = msg.Polar
            let angle = polar.Theta * Math.PI / 180.0
            { // Cartesian record
                X=polar.Radius * Math.Cos(angle)
                Y = polar.Radius * Math.Sin(angle)
            } |> msg.ReplyChannel.Reply
            do! msgLoop ()
        }
        msgLoop ()
    )

let atof = System.Single.Parse >> float

let interact (solver:MailboxProcessor<PolarMessage>) =
    while true do
        printf "Radius and angle: "
        match Console.ReadLine () with
        | null -> () // ignore null
        | line -> 
            match line.Split([|' '|], StringSplitOptions.RemoveEmptyEntries) with 
            | [|radius; angle|] -> 
                let polar = { Radius=radius |> atof; Theta=angle |> atof }
                let buildMsg = fun channel -> { PolarMessage.Polar=polar; ReplyChannel=channel }
                let reply = solver.PostAndReply buildMsg
                printfn "Polar radius=%.02f theta=%.02f degrees Cartesian x=%.02f y=%.02f" polar.Radius polar.Theta reply.X reply.Y
            | _ -> printfn "invalid input"


[<EntryPoint>]
let main argv =
    printfn "%s" prompt
    interact polarToCartesianSolver
    0
