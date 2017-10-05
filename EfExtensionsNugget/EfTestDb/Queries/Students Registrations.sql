SELECT 
        S.LastName,
	   S.FirstName,
        R.RegistrationDate,
        C.CourseName
FROM 
	   Students	    AS S
    INNER JOIN 
	   Registrations   AS R 
		  ON S.StudentId = R.StudentId
    INNER JOIN 
	   Courses	    AS C 
		  ON R.CourseId = C.CourseId
ORDER BY 1, 3;