import emitter from './eventBus';

const webSocket = () => {
  const ws = new WebSocket('ws://localhost:5000/ws');
  ws.onopen = () => {
    console.log('websocket is connected.');
    ws.send(JSON.stringify({ type: 'connected' }));
  };
  ws.onclose = () => {
    console.log('websocket is closed.');
  };
  ws.onerror = (err) => {
    console.log('websocket error ', err);
  };
  ws.onmessage = (message) => {
    try {
      const evt = JSON.parse(message.data);
      handleEvent(evt);
    } catch (e) {
      console.error(e, message);
    }
  };
  return ws;
};

const handleEvent = (evt: any) => {
  console.log('Message from server', evt);
  emitter.emit(evt.type, evt.message);
};

export default webSocket;
