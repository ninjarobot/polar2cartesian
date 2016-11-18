open System

type Polar = {
    Radius : float
    Theta : float
}

type Cartesian = {
    X : float
    Y : float
}


let prompt = 
    match Environment.OSVersion.Platform with 
    | PlatformID.Unix | PlatformID.MacOSX -> // Linux / OS Xml
        "Ctrl+D"
    | _ ->
        "Ctrl+C, Enter"
    |> sprintf "Enter a radius and an angle (in degrees) or %s to quit."

let createSolver postResponse = 
    printfn "About to start solver."
    MailboxProcessor.Start(fun inbox ->
        printfn "About to start solver loop."
        let rec msgLoop () = async {
            printfn "Waiting for polar."
            let! (polar:Polar) = inbox.Receive ()
            let angle = polar.Theta * Math.PI / 180.0
            let x = polar.Radius * Math.Cos(angle)
            let y = polar.Radius * Math.Sin(angle)
            { X=x; Y=y } |> postResponse
            do! msgLoop ()
        }
        msgLoop ()
    )

let answers sendAnswer =
    MailboxProcessor.Start(fun inbox ->
        printfn "About to start answer loop"
        let rec msgLoop () = async {
            printfn "Waiting for cartesian."
            let! (cartesian:Cartesian) = inbox.Receive()
            cartesian |> sendAnswer
            do! msgLoop ()
        }
        msgLoop ()
    )

let strToFloat = System.Single.Parse >> float

printfn "Creating answer agent"
let answerAgent = answers (fun c -> printfn "X=%f, Y=%f" c.X c.Y)
printfn "Creating question agent, passing answer agent."
let questionAgent = createSolver (fun c -> c |> answerAgent.Post)

let interact (questions:MailboxProcessor<Polar>) (answers:MailboxProcessor<Cartesian>) =
    printfn "Starting interaction between agents."
    while true do
        let line = Console.ReadLine().Split(' ')
        match line with 
        | [|radius; angle|] -> 
            {Polar.Radius = radius |> strToFloat; Theta=angle |> strToFloat}
            |> questions.Post
        | _ -> printfn "Invalid input."


interact questionAgent answerAgent