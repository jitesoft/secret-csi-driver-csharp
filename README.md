# CSI Secret Provider

This project is just a demo (or rather, can I do this?) project.

It contains a mock CSI secret driver for kubernetes, demonstrating how it could be implemented with C#.

The project is not to be used as-is.

## Issues

For kestrel to accept the socket connection from the CSI Driver, a sidecar container was required, 
this might be possible to fix (and I will update this repository if I do), but currently, the
[zetanova/grpc-poxy](https://github.com/Zetanova/grpc-proxy) container is used to forward the socket.

## Licenses

The sourcecode in the repository is licensed under the MIT license, excluding the Proto file, which
uses the Apache License v2.
