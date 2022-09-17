package main

import (
	"flag"
	"log"
)

// datapath is the path to the arinc717 file.
var datapath string

// wps is the words per second of the data recording
var wps int = 2048

var serverEndpoint string

func main() {
	log.Println("Hello to Kiwi, the aircraft data emulator")

	flag.StringVar(&datapath, "datapath", "", "The path of the ARINC717 data recording")
	flag.IntVar(&wps, "wps", 0, "The words per second value of the data")
	flag.StringVar(&serverEndpoint, "endpoint", "", "The endpoint we are going to send the data to. ")

	flag.Parse()

	//confirm things
	if datapath == "" {
		log.Fatal("No datapath set.")
	}
	if wps == 0 {
		log.Fatal("No words per second (wps) set.")
	}
	if serverEndpoint == "" {
		log.Fatal("No server endpoint set.")
	}

	log.Println("Kiwi is set with the following properties: datapath: " + datapath)
}
