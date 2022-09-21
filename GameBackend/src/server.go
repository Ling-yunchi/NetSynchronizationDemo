package main

import (
	"fmt"
	"github.com/panjf2000/gnet"
	"log"
	"reflect"
	"strconv"
	"strings"
)

type ClientState struct {
	ReadBuffer [1024]byte
	Hp         int
	X          float32
	Y          float32
	Z          float32
	EulerY     float32
}

type GameServer struct {
	*gnet.EventServer
	states map[gnet.Conn]*ClientState
}

func NewGameServer() *GameServer {
	return &GameServer{
		states: make(map[gnet.Conn]*ClientState),
	}
}

func (es *GameServer) OnInitComplete(srv gnet.Server) (action gnet.Action) {
	log.Printf("Game server is listening on %s (multi-cores: %t, loops: %d)\n",
		srv.Addr.String(), srv.Multicore, srv.NumEventLoop)
	return
}

func (es *GameServer) React(frame []byte, c gnet.Conn) (out []byte, action gnet.Action) {
	recv := string(frame)
	fmt.Printf("receive %s from %s\n", recv, c.RemoteAddr().String())
	split := strings.Split(recv, "|")
	methodName := "On" + split[0]
	method := reflect.ValueOf(es).MethodByName(methodName)
	if method.IsValid() {
		method.Call([]reflect.Value{reflect.ValueOf(c), reflect.ValueOf(split[1])})
	} else {
		fmt.Println("Invalid method name:", methodName)
	}
	return
}

func (es *GameServer) OnOpened(c gnet.Conn) (out []byte, action gnet.Action) {
	fmt.Printf("%s connected\n", c.RemoteAddr().String())
	return
}

func (es *GameServer) OnClosed(c gnet.Conn, err error) (action gnet.Action) {
	fmt.Printf("%s closed\n", c.RemoteAddr().String())
	delete(es.states, c)
	for conn := range es.states {
		err := conn.AsyncWrite([]byte("Leave|" + c.RemoteAddr().String()))
		if err != nil {
			fmt.Printf("Send %s failed: %v\n", conn.RemoteAddr().String(), err)
		}
	}
	return
}

// ============= event handlers =============

func (es *GameServer) OnEnter(conn gnet.Conn, msg string) {
	fmt.Printf("OnEnter: %s, %s\n", conn.RemoteAddr().String(), msg)
	split := strings.Split(msg, ",")
	//desc := split[0]
	x, _ := strconv.ParseFloat(split[1], 32)
	y, _ := strconv.ParseFloat(split[2], 32)
	z, _ := strconv.ParseFloat(split[3], 32)
	eulerY, _ := strconv.ParseFloat(split[4], 32)
	es.states[conn] = &ClientState{
		Hp:     100,
		X:      float32(x),
		Y:      float32(y),
		Z:      float32(z),
		EulerY: float32(eulerY),
	}

	sendMsg := "Enter|" + msg
	for c := range es.states {
		err := c.AsyncWrite([]byte(sendMsg))
		if err != nil {
			fmt.Printf("Send %s failed: %v\n", c.RemoteAddr().String(), err)
		}
	}
}

func (es *GameServer) OnList(conn gnet.Conn, msg string) {
	fmt.Printf("OnList: %s\n", conn.RemoteAddr().String())
	sb := strings.Builder{}
	sb.WriteString("List|")
	for c, state := range es.states {
		sb.WriteString(fmt.Sprintf("%s,%f,%f,%f,%f,%d,", c.RemoteAddr().String(), state.X, state.Y, state.Z, state.EulerY, state.Hp))
	}
	sendMsg := sb.String()
	err := conn.AsyncWrite([]byte(sendMsg))
	if err != nil {
		fmt.Printf("Send %s failed: %v\n", conn.RemoteAddr().String(), err)
	}
}

func (es *GameServer) OnMove(conn gnet.Conn, msg string) {
	fmt.Printf("OnMove: %s, %s\n", conn.RemoteAddr().String(), msg)
	split := strings.Split(msg, ",")
	//desc := split[0]
	x, _ := strconv.ParseFloat(split[1], 32)
	y, _ := strconv.ParseFloat(split[2], 32)
	z, _ := strconv.ParseFloat(split[3], 32)
	state := es.states[conn]
	state.X = float32(x)
	state.Y = float32(y)
	state.Z = float32(z)

	sendMsg := "Move|" + msg
	for c := range es.states {
		err := c.AsyncWrite([]byte(sendMsg))
		if err != nil {
			fmt.Printf("Send %s failed: %v\n", c.RemoteAddr().String(), err)
		}
	}
}
