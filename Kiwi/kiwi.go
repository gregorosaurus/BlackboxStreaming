package main

import (
	"bytes"
	"encoding/csv"
	"flag"
	"fmt"
	"io"
	"log"
	"net/http"
	"os"
	"path/filepath"
	"regexp"
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
	if wps == 0 && filepath.Ext(datapath) != ".csv" {
		log.Fatal("No words per second (wps) set.")
	}
	if serverEndpoint == "" {
		log.Fatal("No server endpoint set.")
	}

	log.Println("Kiwi is set with the following properties: datapath: " + datapath)

	if filepath.Ext(datapath) == ".csv" {
		startEmulationDecoded()
	} else {
		startEmulation()
	}
}

func startEmulationDecoded() {
	log.Println("Starting decoded FDR emulation")

	decodedCsvFile, err := os.Open(datapath)
	if err != nil {
		log.Fatalf("Unable to open fdr data file: %s", err)
	}
	defer decodedCsvFile.Close()

	httpClient := &http.Client{
		Timeout: time.Second * 5,
	}

	csvReader := csv.NewReader(decodedCsvFile)

	headerValues, err := csvReader.Read()
	if err != nil {
		log.Fatalf("Unable to read header of csv: %s", err)
	}

	dateRegex := regexp.MustCompile(`\d{4}-\d{2}-\d{2}[T\s]\d{2}:\d{2}:\d{2}`)
	firstRow, _ := csvReader.Read()
	indexOfTime := -1
	var currentTime time.Time
	for i, firstRowData := range firstRow {
		if dateRegex.FindString(firstRowData) != "" {
			indexOfTime = i
			currentTime, _ = time.Parse("2006-01-02 15:05:04", strings.ReplaceAll(firstRowData, "T", ""))
			break
		}
	}

	if indexOfTime == -1 {
		log.Fatal("No date found in csv file")
	}

	//okay ready to do the rest of the read

	numberMatchRegex := regexp.MustCompile(`^[-\+]?\d+\.?\d*$`)
	numberLeadingZeroCheck := regexp.MustCompile(`^0\d+`)

	nextTimeToGoTo := currentTime.Add(time.Second)

	var readErr error = nil
	var row []string
	for readErr == nil {
		row, readErr = csvReader.Read()

		rowTime, _ := time.Parse("2006-01-02 15:05:04", strings.ReplaceAll(row[indexOfTime], "T", ""))
		if rowTime.Before(nextTimeToGoTo) {
			continue
		} else {
			nextTimeToGoTo = nextTimeToGoTo.Add(time.Second * 1)
		}

		log.Printf("Sending csv data for time %s\n", rowTime.Format(time.RFC3339))

		//var build up json, dont use a library, pshaw
		var json = "{\n"
		for i, header := range headerValues {
			if header == "" {
				continue
			}
			if numberMatchRegex.FindString(row[i]) != "" {
				numValue := row[i]
				numValue = strings.TrimLeft(numValue, "+")
				if numberLeadingZeroCheck.FindString(numValue) != "" {
					numValue = strings.TrimLeft(numValue, "0")
				}
				json += fmt.Sprintf("\"%s\":[%s],\n", header, numValue) //number
			} else {
				json += fmt.Sprintf("\"%s\":[\"%s\"],\n", header, row[i]) //string
			}
		}
		json = strings.TrimRight(json, ",\n")
		json += "\n}"

		go sendDecodedJsonData(httpClient, json)

		time.Sleep(1 * time.Second) //wait for the next "second" of data
	}

}

func sendDecodedJsonData(httpClient *http.Client, json string) {
	log.Println("sending decoded json data")
	defer log.Println("Sent decoded json data. ")
	var requestUrl = serverEndpoint
	if strings.Contains(requestUrl, "?") {
		requestUrl += "&ident=" + acIdent
	} else {
		requestUrl += "?ident=" + acIdent
	}
	response, err := httpClient.Post(requestUrl, "application/json", strings.NewReader(json))
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
}

func startEmulation() {
	log.Println("Starting FDR stream emulation")

	fdrFile, err := os.Open(datapath)
	if err != nil {
		log.Fatalf("Unable to open fdr data file: %s", err)
	}
	defer fdrFile.Close()

	httpClient := &http.Client{
		Timeout: time.Second * 5,
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
