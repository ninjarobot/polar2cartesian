polar2cartesian
===============

This takes an example of the polar2cartesian sample from _Programming in Go_
and rebuilds it in F#.

polar2cartesian.go in Go is an example of using goroutines for asynchronous 
processing and using channels to communicate between them.  Similar 
functionality can be created in F# using asynchronous workflows wrapped in a
`MailboxProcessor`.  The polar2cartesian.fs module demonstrates how similar
functionality can be achieved, albeit with some minor differences in how
you interact with a mailbox vs. a channel.