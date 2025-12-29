$pwd = ConvertTo-SecureString -String "LotteryTracker2024" -Force -AsPlainText
Export-PfxCertificate -Cert "Cert:\CurrentUser\My\DF6BC9E290874674DE04918A6BAA3655330F0FF7" -FilePath "c:\Users\ct_kl\source\repos\LotteryTrack\LotteryTracker.pfx" -Password $pwd
Export-Certificate -Cert "Cert:\CurrentUser\My\DF6BC9E290874674DE04918A6BAA3655330F0FF7" -FilePath "c:\Users\ct_kl\source\repos\LotteryTrack\LotteryTracker.cer"
