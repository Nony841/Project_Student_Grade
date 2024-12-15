module InputForm
open System.Windows.Forms
open System.Drawing
open System
open StudentManagement

type InputForm(title: string, fields: (string * string) list, studentOption: option<Student>) as this =
    inherit Form()

    let mutable inputs = Map.empty
    let mutable students = loadStudents "students.txt"

    // OK and Cancel buttons
    let btnOk = new Button(Text = "OK", DialogResult = DialogResult.OK, BackColor = Color.LightGreen, Width = 100)
    let btnCancel = new Button(Text = "Cancel", DialogResult = DialogResult.Cancel, BackColor = Color.LightCoral, Width = 100)

    do
        this.Text <- title
        this.Width <- 300
        this.Height <- 300
        this.StartPosition <- FormStartPosition.CenterParent
        this.BackColor <- Color.WhiteSmoke

        let panel = new FlowLayoutPanel(Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = Padding(10))
        panel.AutoScroll <- true

        let lblFont = new Font("Arial", 10.0f, FontStyle.Regular)
        let txtFont = new Font("Arial", 10.0f, FontStyle.Regular)
        
        fields |> List.iter (fun (labelText, defaultValue) ->
            let lbl = new Label(Text = labelText, Font = lblFont, AutoSize = true)
            let txt = new TextBox(Text = defaultValue, Font = txtFont, Width = 250)
            txt.Margin <- Padding(0, 5, 0, 5) 
            panel.Controls.Add(lbl)
            panel.Controls.Add(txt)
            inputs <- inputs.Add(labelText, txt)
        )

        btnOk.Margin <- Padding(10)
        btnCancel.Margin <- Padding(10)
        btnOk.Anchor <- AnchorStyles.Bottom
        btnCancel.Anchor <- AnchorStyles.Bottom

        let buttonPanel = new FlowLayoutPanel(Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, Padding = Padding(10), Height = 50)
        buttonPanel.Controls.Add(btnOk)
        buttonPanel.Controls.Add(btnCancel)

        this.Controls.Add(panel)
        this.Controls.Add(buttonPanel)

    member this.GetValues() =
        inputs |> Map.map (fun _ txt -> txt.Text)

    member this.SaveStudentChanges() =
        let studentData = this.GetValues()
    
        match studentOption with
        | Some student -> 
            let gradesStr = studentData.["Grades"]
 
            let validGrades =
                gradesStr.Split([| ';' |], StringSplitOptions.RemoveEmptyEntries)
                |> Array.map (fun gradeStr -> 
                    match System.Double.TryParse(gradeStr) with
                    | (true, grade) when grade >= 0.0 && grade <= 100.0 -> Some grade
                    | _ -> None)
                |> Array.choose id

            if validGrades.Length = gradesStr.Split([| ';' |], StringSplitOptions.RemoveEmptyEntries).Length then
                let updatedStudent = {
                    student with
                        Name = studentData.["Name"]
                        Grades = List.ofArray validGrades
                }

                let updatedStudents = 
                    students
                    |> List.filter (fun s -> s.Id <> updatedStudent.Id)
                    |> List.append [updatedStudent] 
             
                saveStudents "students.txt" updatedStudents
                MessageBox.Show("Student data updated successfully.") |> ignore
            else
                MessageBox.Show("Invalid grades entered. Please enter valid numbers between 0 and 100.") |> ignore

        | None -> 
            MessageBox.Show("No student selected to update.") |> ignore
