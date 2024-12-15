module UserDashboard
open System
open System.Windows.Forms
open System.Drawing
open System.IO

type Student = { Id: int; Name: string; Grades: int list }

let students = 
    let filePath = "students.txt"
    if File.Exists(filePath) then
        File.ReadLines(filePath)
        |> Seq.map (fun line ->
            let parts = line.Split(',')
            let id = int parts.[0]
            let name = parts.[1]
            let grades = parts.[2].Split(';') |> Array.map int |> Array.toList 
            { Id = id; Name = name; Grades = grades })
        |> Seq.toList
    else
        []

let getStudentById (students: Student list) (id: int) =
    students |> List.tryFind (fun student -> student.Id = id)

let calculateAverage grades =
    if List.isEmpty grades then None
    else Some (List.averageBy float grades)

type MainForm(studentId: int) as this =
    inherit Form()

    
    let lblTitle = new Label(Text = "Student Grades Management System ", 
                                  Font = new Font("Arial", 18.0f, FontStyle.Bold), 
                                  Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter,
                                  Height = 30, BackColor = Color.MidnightBlue, ForeColor = Color.White)
    let lbluserdashboard = new Label(Text = "User Dashboard ", 
                                  Font = new Font("Arial", 18.0f, FontStyle.Bold), 
                                  Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter,
                                  Height = 30, ForeColor = Color.Black)    
    let panelActions = new Panel(Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke, Padding = Padding(10))

    let btnViewStudentInfo = new Button(Text = "View Student Information", BackColor = Color.LightBlue, Width = 150)
    let btnViewGrades = new Button(Text = "View Student Average Grade", BackColor = Color.LightCoral, Width = 150)
    
    let controlPanel = new FlowLayoutPanel(
        Dock = DockStyle.Fill, 
        FlowDirection = FlowDirection.TopDown, 
        WrapContents = false, 
        AutoScroll = true, 
        Padding = Padding(20, 70, 20, 20), 
        Width = this.ClientSize.Width,
        Height = 200
    )

    do
        this.Text <- "Student Management System"
        this.Width <- 600
        this.Height <- 700
        this.StartPosition <- FormStartPosition.CenterScreen
        this.BackColor <- Color.WhiteSmoke
   
        controlPanel.Controls.AddRange([| 
            btnViewStudentInfo; btnViewGrades
        |])

        let centerButtons (control: Control) =
            control.Width <- controlPanel.ClientSize.Width - 60
            control.Margin <- Padding(10)  

        controlPanel.Controls
        |> Seq.cast<Control>
        |> Seq.iter centerButtons

        this.Controls.Add(lbluserdashboard)
        this.Controls.Add(lblTitle)
        this.Controls.Add(controlPanel)

        let centerButtons (control: Control) =
            control.Width <- controlPanel.ClientSize.Width - 60
            control.Margin <- Padding(10)

        controlPanel.Controls
        |> Seq.cast<Control>
        |> Seq.iter centerButtons

        btnViewStudentInfo.Click.Add(fun _ -> 
            match getStudentById students studentId with
            | Some student -> 
                MessageBox.Show(sprintf "ID: %d\nName: %s\nGrades: %s" student.Id student.Name (String.Join(", ", student.Grades))) |> ignore
            | None -> MessageBox.Show("Student not found.") |> ignore
        )

        btnViewGrades.Click.Add(fun _ -> 
            match getStudentById students studentId with
            | Some student -> 
                match calculateAverage student.Grades with
                | Some avg -> MessageBox.Show(sprintf "Your average grade: %.2f" avg) |> ignore
                | None -> MessageBox.Show("No grades available.") |> ignore
            | None -> MessageBox.Show("Student not found.") |> ignore
        )