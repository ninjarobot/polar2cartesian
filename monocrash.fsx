
// Simple tuple works fine:
//type Rec = int * int

// But a record crashes the runtime
type Rec = {
    A : int
    B : int
}

MailboxProcessor<Rec>.Start(fun inbox ->
    let rec loop () = async {
        let! r = inbox.Receive ()
        do! loop ()
    }        
    loop ()
)