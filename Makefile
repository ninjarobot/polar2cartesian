all:
	go build polar2cartesian.go
	fsharpc polar2cartesian.fs

clean:
	rm polar2cartesian
	rm polar2cartesian.exe
