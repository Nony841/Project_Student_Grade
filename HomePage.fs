module HomePage
open UserRoles
open System.Windows.Forms
open System.Drawing

type MainForm() as this =
    inherit Form()

    let lblTitle = new Label(
        Text = "Welcome to the Student Grades Management System",
        Font = new Font("Arial", 18.0f, FontStyle.Bold),
        Dock = DockStyle.Top,
        TextAlign = ContentAlignment.MiddleCenter,
        Height = 50,
        BackColor = Color.MidnightBlue,
        ForeColor = Color.White
    )

    let lblRoleSelection = new Label(
        Text = "Please select your role",
        Font = new Font("Arial", 14.0f, FontStyle.Regular),
        Dock = DockStyle.Top,
        TextAlign = ContentAlignment.MiddleCenter,
        Height = 30,
        ForeColor = Color.Black
    )

    let btnAdmin = new Button(
        Text = "Admin",
        BackColor = Color.LightGreen,
        Font = new Font("Arial", 12.0f, FontStyle.Regular),
        Width = 150,
        Height = 40
    )

    let btnUser = new Button(
        Text = "User",
        BackColor = Color.LightBlue,
        Font = new Font("Arial", 12.0f, FontStyle.Regular),
        Width = 150,
        Height = 40
    )

    do
        this.Text <- "Home Page"
        this.Width <- 600
        this.Height <- 400
        this.StartPosition <- FormStartPosition.CenterScreen
        this.BackColor <- Color.WhiteSmoke
        this.Controls.Add(lblRoleSelection)
        this.Controls.Add(lblTitle)
        this.Controls.Add(btnAdmin)
        this.Controls.Add(btnUser)

        btnAdmin.SendToBack()
        btnUser.SendToBack()

        let centerComponents () =
            lblTitle.Top <- 10
            lblRoleSelection.Top <- lblTitle.Bottom + 10
            lblRoleSelection.Left <- (this.ClientSize.Width - lblRoleSelection.Width) / 2

            btnAdmin.Left <- (this.ClientSize.Width - btnAdmin.Width) / 2
            btnAdmin.Top <- (this.ClientSize.Height - (btnAdmin.Height * 2 + 20)) / 2

            btnUser.Left <- btnAdmin.Left
            btnUser.Top <- btnAdmin.Bottom + 10

        this.Resize.Add(fun _ -> centerComponents())

        centerComponents()

        btnAdmin.Click.Add(fun _ ->
            let authForm = new AuthForm("Admin", fun _ ->
                this.Hide()
                let adminForm = new AdminDashboard.MainForm()
                adminForm.FormClosed.Add(fun _ -> this.Show())
                adminForm.Show()
            )
            authForm.ShowDialog() |> ignore
        )
        
        btnUser.Click.Add(fun _ ->
            let authForm = new AuthForm("User", fun id ->
                this.Hide()
                let userDashboard = new UserDashboard.MainForm(id)  
                userDashboard.FormClosed.Add(fun _ -> this.Show())   
                userDashboard.Show()  
            )
            authForm.ShowDialog() |> ignore
        )
      