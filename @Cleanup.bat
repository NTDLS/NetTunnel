@Echo Off


attrib *.* -h -a -r -s

rd .vs /s /q

rd NetTunnel.Client\obj /s /q
rd NetTunnel.Client\bin /s /q
rd NetTunnel.Client\.vs /s /q

rd NetTunnel.Library\obj /s /q
rd NetTunnel.Library\bin /s /q
rd NetTunnel.Library\.vs /s /q

rd NetTunnel.Hub\obj /s /q
rd NetTunnel.Hub\bin /s /q
rd NetTunnel.Hub\.vs /s /q

rd NetTunnel.Service\obj /s /q
rd NetTunnel.Service\bin /s /q
rd NetTunnel.Service\.vs /s /q

rd packages /s /q
rd setup\Output /s /q

