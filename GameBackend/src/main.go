package main

import (
	"github.com/panjf2000/gnet"
	"log"
)

func main() {
	gameServer := NewGameServer()
	log.Fatal(gnet.Serve(gameServer, "tcp://:9000", gnet.WithMulticore(true)))
}
