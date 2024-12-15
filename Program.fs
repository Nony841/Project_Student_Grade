open System
open System.Windows.Forms
open HomePage
// Assuming you have a `HomePage` class defined as the starting point of your application
[<STAThread>]
[<EntryPoint>]
let main argv =
    Application.EnableVisualStyles()
    Application.SetCompatibleTextRenderingDefault(false)
    Application.Run(new HomePage.MainForm())  // Start with the HomePage form
    0

