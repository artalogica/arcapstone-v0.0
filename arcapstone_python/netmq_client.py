import time

import cv2
import zmq

context = zmq.Context()
socket = context.socket(zmq.PUSH)  # <- the PUSH socket
socket.bind('tcp://*:12345')

capture = cv2.VideoCapture(0)

while True:
    start = time.time()

    _, frame = capture.read()
    data = {
        'bool': True,
        'int': 123,
        'str': 'Hello there!',
        'image': cv2.imencode('.jpg', frame)[1].ravel().tolist()
    }

    socket.send_json(data)

    end = time.time()
    print('FPS:', 1 / (end - start))

    cv2.imshow('Webcam', frame)
    cv2.waitKey(delay=1)
