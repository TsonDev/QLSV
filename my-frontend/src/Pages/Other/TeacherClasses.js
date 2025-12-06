import React, { useEffect, useState } from "react";
import { Link } from "react-router-dom";

export default function TeacherClasses() {
    const [classes, setClasses] = useState([]);

    const loadClasses = async () => {
        const res = await fetch("http://localhost:7287/api/studentSubjects/class/status");
        const data = await res.json();
        setClasses(data);
    };

    useEffect(() => {
        loadClasses();
    }, []);

    return (
        <div style={{ padding: "20px" }}>
            <h2>Danh sách lớp của giảng viên</h2>

            <ul>
                {classes.map((cls) => (
                    <li key={cls.classId}>
                        <Link to={`/teacher/class/${cls.classId}`}>
                            {cls.className} (ID: {cls.classId})
                        </Link>
                    </li>
                ))}
            </ul>
        </div>
    );
}
