cd D:/Training/gRPC/MeterReader/src/MeterReaderClientPython

python -m grpc.tools.protoc `
       -I ../ `
       --python_out ./ `
       --grpc_python_out ./ `
       meterservice.proto