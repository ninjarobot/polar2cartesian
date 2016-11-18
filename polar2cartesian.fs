open System

type Coordinate =
    | Polar of Radius:float * Theta:float
    | Cartesian of X:float * Y:float

type Message = {
    ReplyChannel: AsyncReplyChannel<Coordinate>
    Coordinate: Coordinate
}

let prompt = 
    match Environment.OSVersion.Platform with 
    | PlatformID.Unix | PlatformID.MacOSX -> // Linux / OS X
        "Ctrl+C"
    | _ ->
        "Ctrl+C, Enter"
    |> sprintf "Enter a radius and an angle (in degrees) or %s to quit."

let polarToCartesianSolver = 
    MailboxProcessor<Message>.Start(fun inbox ->
        let rec msgLoop () = async {
            let! msg = inbox.Receive ()
            match msg.Coordinate with
            | Polar (radius, theta) ->
                let angle = theta * Math.PI / 180.0
                Cartesian (X=radius * Math.Cos angle, Y=radius * Math.Sin angle)
                |> msg.ReplyChannel.Reply
            | Cartesian (x, y) -> 
                Polar (Radius=sqrt (x**2.0 + y**2.0), Theta=atan (y/x) * 180.0 / Math.PI)
                |> msg.ReplyChannel.Reply
            do! msgLoop ()
        }
        msgLoop ()
    )

let atof = System.Single.Parse >> float

let interact (solver:MailboxProcessor<Message>) =
    while true do
        printf "Radius and angle: "
        match Console.ReadLine () with
        | null -> () // ignore null
        | line -> 
            match line.Split ([|' '|], StringSplitOptions.RemoveEmptyEntries) with 
            | [|radius; angle|] -> 
                let coordinate = Polar (Radius=atof radius, Theta=atof angle)
                let buildMsg coord = fun channel -> { Message.Coordinate=coord; ReplyChannel=channel }
                let translated = buildMsg coordinate |> solver.PostAndReply
                let andBack = buildMsg translated |> solver.PostAndReply
                printfn "Original: %A Translated: %A Back: %A" coordinate translated andBack
            | _ -> printfn "invalid input"


[<EntryPoint>]
let main argv =
    printfn "%s" prompt
    interact polarToCartesianSolver
    0
