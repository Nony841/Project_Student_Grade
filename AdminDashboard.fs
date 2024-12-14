module public AdminDashboard
open System
open System.Windows.Forms
open System.Drawing
open System.IO
open InputForm
open StudentManagement
open UserRoles

let studentFilePath = "students.txt"

let loadStudentsFromFile () =
    if File.Exists(studentFilePath) then
        let lines = File.ReadAllLines(studentFilePath)
        lines |> Array.map (fun line -> 
            let parts = line.Split(',')
            let id = Int32.Parse(parts.[0])
            let name = parts.[1]
            let grades = parts.[2].Split(';') |> Array.map float |> Array.toList
            { Id = id; Name = name; Grades = grades }
        ) |> Array.toList
    else
        []

let saveStudentsToFile students =
    let lines = students |> List.map (fun student -> 
        let grades = String.Join(";", student.Grades)
        sprintf "%d,%s,%s" student.Id student.Name grades
    )
    File.WriteAllLines(studentFilePath, lines)

let mutable students = loadStudentsFromFile()

type MainForm() as this =
    inherit Form()
   
    let mutable userRole = Admin

    let lblTitle = new Label(Text = "Student Grades Management System ", 
                                  Font = new Font("Arial", 18.0f, FontStyle.Bold), 
                                  Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter,
                                  Height = 30, BackColor = Color.MidnightBlue, ForeColor = Color.White)
    let lbladmindashboard = new Label(Text = "Admin Dashboard ", 
                                  Font = new Font("Arial", 18.0f, FontStyle.Bold), 
                                  Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter,
                                  Height = 30, ForeColor = Color.Black)   
    
    let panelActions = new Panel(Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke, Padding = Padding(10))

    let btnAddStudent = new Button(Text = "Add Student", BackColor = Color.LightBlue, Width = 150)
    let btnRemoveStudent = new Button(Text = "Remove Student", BackColor = Color.LightCoral, Width = 150)
    let btnEditStudent = new Button(Text = "Edit Student", BackColor = Color.LightGoldenrodYellow, Width = 150)
    let btnGetStudent = new Button(Text = "Get Student By ID", BackColor = Color.LightPink, Width = 150)
    let btnDisplayAllStudents = new Button(Text = "Display All Students", BackColor = Color.LightGray, Width = 150)
    let btnViewGrades = new Button(Text = "View Class Stats", BackColor = Color.LightSlateGray, ForeColor = Color.White, Width = 150)

    let controlPanel = new FlowLayoutPanel(
        Dock = DockStyle.Fill, 
        FlowDirection = FlowDirection.TopDown, 
        WrapContents = false, 
        AutoScroll = true, 
        Padding = Padding(20, 70, 20, 20),  
        Width = this.ClientSize.Width,
        Height = 300
    )
    
    let updateStats() =
        if students.Length = 0 then
            MessageBox.Show("No students available.") |> ignore
        else
            let (passRate, failRate) = passFailRate students 50.0
            let highest = getHighestGrade students
            let lowest = getLowestGrade students
            let statsMessage = String.concat "\n\n" [
                sprintf "Pass Rate: %.2f%%        " passRate
                sprintf "Fail Rate: %.2f%%        " failRate
                sprintf "Highest Grade: %.2f         " (List.max highest.Grades)
                sprintf "Lowest Grade: %.2f          " (List.min lowest.Grades)
            ]

            let statsForm = new Form(Width = 390, Height = 250, Text = "Student Statistics")
            let textBox = new TextBox(
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Arial", 12.0f),
                Text = statsMessage,
                ScrollBars = ScrollBars.Vertical
            )
            statsForm.Controls.Add(textBox)
            statsForm.ShowDialog() |> ignore

    do
        this.Text <- "Student Management System"
        this.Width <- 600
        this.Height <- 700
        this.StartPosition <- FormStartPosition.CenterScreen
        this.BackColor <- Color.WhiteSmoke
   
        controlPanel.Controls.AddRange([| 
            btnAddStudent; btnRemoveStudent; 
            btnEditStudent; btnGetStudent; btnDisplayAllStudents; btnViewGrades
        |])

        let centerButtons (control: Control) =
            control.Width <- controlPanel.ClientSize.Width - 60
            control.Margin <- Padding(10)  

        controlPanel.Controls
        |> Seq.cast<Control>
        |> Seq.iter centerButtons
        this.Controls.Add(lbladmindashboard)
        this.Controls.Add(lblTitle)
        this.Controls.Add(controlPanel)

        let centerButtons (control: Control) =
            control.Width <- controlPanel.ClientSize.Width - 60
            control.Margin <- Padding(10)

        controlPanel.Controls
        |> Seq.cast<Control>
        |> Seq.iter centerButtons
        btnAddStudent.Click.Add(fun _ -> 
            if isAdmin userRole then
                let inputForm = new InputForm(
                    "Add Student",
                    [("Student ID", ""); ("Student Name", ""); ("Grades (comma-separated)", "")],
                    None 
                )
                if inputForm.ShowDialog() = DialogResult.OK then
                    let values = inputForm.GetValues()
                    match Int32.TryParse(values.["Student ID"]), values.["Student Name"], values.["Grades (comma-separated)"] with
                    | (true, id), name, gradesText when not (String.IsNullOrWhiteSpace(name)) && not (String.IsNullOrWhiteSpace(gradesText)) -> 
                        try
                            let grades = gradesText.Split(',') |> Array.map float |> Array.toList
                            students <- addStudent students { Id = id; Name = name; Grades = grades }
                            saveStudentsToFile students 
                            MessageBox.Show("Student added successfully!") |> ignore
                        with
                        | _ -> MessageBox.Show("Invalid grades format. Please try again.") |> ignore
                    | _ -> MessageBox.Show("Invalid input. Please try again.") |> ignore
            else
                MessageBox.Show("You do not have permission to add students.") |> ignore
        )

        btnRemoveStudent.Click.Add(fun _ -> 
            if isAdmin userRole then
                let inputForm = new InputForm(
                    "Remove Student",
                    [("Student ID", "")],
                    None 
                )
                if inputForm.ShowDialog() = DialogResult.OK then
                    let values = inputForm.GetValues()
                    match Int32.TryParse(values.["Student ID"]) with
                    | (true, id) when getStudentById students id |> Option.isSome -> 
                        students <- removeStudent students id
                        saveStudentsToFile students 
                        MessageBox.Show("Student removed successfully!") |> ignore
                    | _ -> MessageBox.Show("Student not found or invalid ID.") |> ignore
            else
                MessageBox.Show("You do not have permission to remove students.") |> ignore
        )

        btnEditStudent.Click.Add(fun _ -> 
            if isAdmin userRole then
                let inputForm = new InputForm(
                    "Edit Student",
                    [("Student ID", ""); ("New Name", ""); ("New Grades (comma-separated)", "")],
                    None 
                )
                if inputForm.ShowDialog() = DialogResult.OK then
                    let values = inputForm.GetValues()
                    match Int32.TryParse(values.["Student ID"]), values.["New Name"], values.["New Grades (comma-separated)"] with
                    | (true, id), newName, gradesText when not (String.IsNullOrWhiteSpace(newName)) -> 
                        let newGrades = 
                            if not (String.IsNullOrWhiteSpace(gradesText)) then
                                try
                                    gradesText.Split(',') |> Array.map float |> Array.toList
                                with
                                | _ -> []
                            else []
                
                        match getStudentById students id with
                        | Some student -> 
                            students <- updateStudent students id newName (if List.isEmpty newGrades then None else Some newGrades)
                            saveStudentsToFile students 
                            MessageBox.Show("Student name and/or grades updated!") |> ignore
                        | None -> MessageBox.Show("Student not found.") |> ignore
                    | _ -> MessageBox.Show("Invalid input. Please try again.") |> ignore
            else
                MessageBox.Show("You do not have permission to edit student names.") |> ignore
        )

        btnGetStudent.Click.Add(fun _ -> 
            let inputForm = new InputForm(
                "Get Student By ID",
                [("Student ID", "")],
                None 
            )
            if inputForm.ShowDialog() = DialogResult.OK then
                let values = inputForm.GetValues()
                match Int32.TryParse(values.["Student ID"]) with
                | (true, id) -> 
                    match getStudentById students id with
                    | Some student -> 
                        let avgGrade = 
                            match calculateAverage student.Grades with
                            | Some avg -> sprintf "Average Grade: %.2f" avg
                            | None -> "No grades available."
                        MessageBox.Show(sprintf "ID: %d\nName: %s\nGrades: %s\n%s" student.Id student.Name (String.Join(", ", student.Grades)) avgGrade) |> ignore
                    | None -> MessageBox.Show("Student not found.") |> ignore
                | _ -> MessageBox.Show("Invalid ID.") |> ignore
        )

        btnDisplayAllStudents.Click.Add(fun _ -> 
            if students.Length = 0 then
                MessageBox.Show("No students available.") |> ignore
            else
                let allStudents = 
                    students 
                    |> List.sortBy (fun s -> s.Id) 
                    |> List.map (fun s -> 
                        let avgGrade = 
                            match calculateAverage s.Grades with
                            | Some avg -> sprintf "Average: %.2f" avg
                            | None -> "Average: N/A"
                        sprintf """
            ID: %d
            Name: %s
            Grades: %s
            %s
            """ 
                            s.Id
                            s.Name 
                            (String.Join(", ", s.Grades)) 
                            avgGrade
                    )

                let studentDetailsForm = new Form(Width = 500, Height = 500, Text = "Student Details")
                let textBox = new TextBox(Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, Text = String.Join("\n", allStudents), ScrollBars = ScrollBars.Vertical)
                studentDetailsForm.Controls.Add(textBox)
                studentDetailsForm.ShowDialog() |> ignore
        )

        btnViewGrades.Click.Add(fun _ -> updateStats()) 
