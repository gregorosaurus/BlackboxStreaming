# Blackbox Streaming Service

This repo is a dmonstration of how to build a service using 
Azure/Microsoft technologies to handle the real-time QAR/FDR streaming 
data.  


# Components

| Component Name | Description |
|----------------|-------------|
| Blackbird | Blackbird is the function responsible for receiving data from aircraft. It is an Azure Function, and does two things: 1) saves the data in Azure Data Lake Storage, and 2) Sends the subframe data to service bus for downstream processes |
| Kiwi | Kiwi is an aircraft data emulator that sends data to Blackbird. This is a relatively simple program, just iterating through a data file |
| Hawk | Hawk is responsible for decoding the FDR data. | 
| Loon | Loon is the web application available to view real time FDR data |