module StudentManagement
open System
open System.IO

type Student = {
    Id: int
    Name: string
    Grades: float list
}

let loadStudents filePath =
    if File.Exists(filePath) then
        File.ReadAllLines(filePath)
        |> Array.choose (fun line -> 
            try
                let parts = line.Split(',')
                if parts.Length >= 3 then
                    let id = int parts.[0]
                    let name = parts.[1]
                    let grades = parts.[2].Split(';') |> Array.map float |> List.ofArray  
                    Some { Id = id; Name = name; Grades = grades }
                else None
            with
            | :? FormatException as ex -> 
                printfn "Error reading line '%s': %s" line ex.Message
                None
        )
        |> Array.toList
    else
        [] 

let saveStudents filePath students =
    let lines = students |> List.map (fun student -> 
        let grades = student.Grades |> List.map string |> String.concat ";" 
        $"{student.Id},{student.Name},{grades}"
    )
    File.WriteAllLines(filePath, lines)

let mutable students = loadStudents "students.txt"

let addStudent students student =
    let updatedStudents = students |> List.filter (fun s -> s.Id <> student.Id)
    let updatedList = student :: updatedStudents
    saveStudents "students.txt" updatedList
    updatedList

let updateStudent students studentId newName newGrades =
    let updatedStudents = 
        students |> List.map (fun student -> 
            if student.Id = studentId then 
                { student with 
                    Name = newName
                    Grades = match newGrades with
                             | Some grades -> grades  
                             | None -> student.Grades  
                }
            else student)
    saveStudents "students.txt" updatedStudents
    updatedStudents

let removeStudent students studentId =
    let updatedStudents = students |> List.filter (fun student -> student.Id <> studentId)
    saveStudents "students.txt" updatedStudents
    updatedStudents

let getStudentById students studentId =
    students |> List.tryFind (fun student -> student.Id = studentId)

let calculateAverage grades =
    if List.isEmpty grades then None
    else Some (List.average grades)

let passFailRate students passingGrade =
    let passed = students |> List.filter (fun student -> 
        match calculateAverage student.Grades with
        | Some avg when avg >= passingGrade -> true
        | _ -> false)
    
    let total = List.length students
    let passedCount = List.length passed
    let failedCount = total - passedCount

    if total > 0 then
        let passRate = (float passedCount / float total) * 100.0
        let failRate = (float failedCount / float total) * 100.0
        (passRate, failRate)
    else
        (0.0, 0.0) 

let getHighestGrade students =
    students |> List.maxBy (fun student -> List.max student.Grades)

let getLowestGrade students =
    students |> List.minBy (fun student -> List.min student.Grades)
