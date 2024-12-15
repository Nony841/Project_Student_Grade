module UserRoles
open System
open System.Windows.Forms
open System.Drawing
open StudentManagement   

type UserRole = Admin | User

let isAdmin role =
    match role with
    | Admin -> true
    | User -> false

type AuthForm(role: string, onSuccess: int -> unit) as this =
    inherit Form()

    let lblMessage = new Label(
        Text = sprintf "Please enter your %s Data" role,
        Font = new Font("Arial", 14.0f, FontStyle.Bold),
        Dock = DockStyle.Top,
        Height = 40,
        TextAlign = ContentAlignment.MiddleCenter,
        ForeColor = Color.DarkSlateGray
    )

    let lblUsername = new Label(Text = "Username:", Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleLeft)
    let txtUsername = new TextBox(Dock = DockStyle.Top, Padding = new Padding(5), Height = 30)
    
    let lblPasswordOrId = new Label()
    do
        lblPasswordOrId.Text <- if role = "Admin" then "Password:" else "ID:"
        lblPasswordOrId.Dock <- DockStyle.Top
        lblPasswordOrId.TextAlign <- ContentAlignment.MiddleLeft
        lblPasswordOrId.Padding <- new Padding(5)

    let txtPasswordOrId = new TextBox(Dock = DockStyle.Top, Padding = new Padding(5), Height = 30)
    do if role = "Admin" || role = "User" then txtPasswordOrId.PasswordChar <- '*' 
    
    let btnSubmit = new Button(
        Text = "Submit",
        Dock = DockStyle.Bottom,
        Height = 45,
        BackColor = Color.LightBlue,
        Font = new Font("Arial", 12.0f, FontStyle.Bold),
        FlatStyle = FlatStyle.Flat
    )
    do
        btnSubmit.FlatAppearance.BorderSize <- 0
        btnSubmit.ForeColor <- Color.Black

    let panelContainer = new Panel(Dock = DockStyle.Fill, Padding = new Padding(20))
    do
        panelContainer.BackColor <- Color.White
        panelContainer.Controls.AddRange([| btnSubmit; txtPasswordOrId; lblPasswordOrId; txtUsername; lblUsername; lblMessage |])
        this.Controls.Add(panelContainer)

    do
        this.Text <- sprintf "%s Authentication" role
        this.Width <- 400
        this.Height <- 350
        this.StartPosition <- FormStartPosition.CenterScreen
        this.FormBorderStyle <- FormBorderStyle.FixedDialog
        this.MaximizeBox <- false
        this.MinimizeBox <- false

        btnSubmit.Click.Add(fun _ -> 
            let username = txtUsername.Text.ToLower()

            let studentsFromFile = loadStudents "students.txt"

            if role = "Admin" then
                if username = "admin" && txtPasswordOrId.Text = "1234" then 
                    onSuccess(-1)  
                    this.Close()   
                else
                    MessageBox.Show("Invalid username or password.") |> ignore
            else
                match Int32.TryParse(txtPasswordOrId.Text) with
                | true, id -> 
                    let student = studentsFromFile |> List.tryFind (fun s -> s.Id = id && s.Name.ToLower() = username)
                    match student with
                    | Some _ -> 
                        onSuccess(id)  
                        this.Close()   
                    | None -> MessageBox.Show("Invalid username or ID.") |> ignore
                | _ -> MessageBox.Show("Invalid ID format.") |> ignore
        )