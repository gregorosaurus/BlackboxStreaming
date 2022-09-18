package main

import (
	"bytes"
	"flag"
	"io"
	"log"
	"net/http"
	"os"
	"strings"
	"time"
)

// datapath is the path to the arinc717 file.
var datapath string

// wps is the words per second of the data recording
var wps int = 2048

var serverEndpoint string

var acIdent string

var minutesToSkip int = 0

func main() {
	log.Println("Hello to Kiwi, the aircraft data emulator")

	flag.StringVar(&datapath, "datapath", "", "The path of the ARINC717 data recording")
	flag.IntVar(&wps, "wps", 0, "The words per second value of the data")
	flag.StringVar(&serverEndpoint, "endpoint", "", "The endpoint we are going to send the data to. ")
	flag.StringVar(&acIdent, "acident", "CKIWI", "The aircraft identifier to emulate")
	flag.IntVar(&minutesToSkip, "skipmins", 0, "Number of minutes to start at in the data file")
	flag.Parse()

	//confirm things
	if datapath == "" {
		log.Fatal("No datapath set.")
	} else if _, err := os.Stat(datapath); err != nil {
		log.Fatal("Error: Unable to access data")
	}
	if wps == 0 {
		log.Fatal("No words per second (wps) set.")
	}
	if serverEndpoint == "" {
		log.Fatal("No server endpoint set.")
	}

	log.Println("Kiwi is set with the following properties: datapath: " + datapath)

	startEmulation()
}

func startEmulation() {
	log.Println("Starting FDR stream emulation")

	fdrFile, err := os.Open(datapath)
	if err != nil {
		log.Fatalf("Unable to open fdr data file: %s", err)
	}

	httpClient := &http.Client{
		Timeout: time.Second * 3,
	}

	var buffersize int = wps * 2
	buffer := make([]byte, buffersize)

	//read 10 mins into the flight, testing
	i := 0
	for i < minutesToSkip*60*4 {
		fdrFile.Read(buffer)
		i += 1
	}

	for {
		bytesRead, err := fdrFile.Read(buffer)
		if bytesRead != wps*2 {
			log.Fatal("Invalid byte read")
		}
		if err != nil && err != io.EOF {
			log.Fatalf("Unable to read file into buffer: %s", err)
		}

		//we have read the data, now send it to the endpoint
		var requestUrl = serverEndpoint
		if strings.Contains(requestUrl, "?") {
			requestUrl += "&ident=" + acIdent
		} else {
			requestUrl += "?ident=" + acIdent
		}
		response, err := httpClient.Post(requestUrl, "application/octet-stream", bytes.NewReader(buffer))
		if err != nil {
			log.Fatalf("Unable to send data to endpoint: %s", err)
		}

		if response.StatusCode != 200 {
			if b, err := io.ReadAll(response.Body); err == nil {
				log.Fatalf("Invalid response when sending fdr data: %d\n%s", response.StatusCode, string(b))
			} else {
				log.Fatalf("Invalid response when sending fdr data: %d", response.StatusCode)
			}

		}

		response.Body.Close()

		time.Sleep(1 * time.Second) //wait for the next subframe.
	}
}
