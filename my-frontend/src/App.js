import React from "react";
import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import Home from "./Pages/Home";
import Subjects from "./Pages/Subjects";
import ScoreInputPageWrapper from "./Pages/ScoreInputPageWrapper";
import ApprovePage from "./Pages/ApprovePage";

function App() {
    return (
        <Router>
            <nav
                style={{
                    backgroundColor: "#007bff",
                    padding: "10px 0",
                    marginBottom: "20px",
                }}
            >
                <ul
                    style={{
                        listStyle: "none",
                        display: "flex",
                        justifyContent: "center",
                        gap: "20px",
                        margin: 0,
                        padding: 0,
                    }}
                >
                    <li>
                        <Link to="/" style={{ color: "white", textDecoration: "none" }}>
                            Trang chủ
                        </Link>
                    </li>
                    <li>
                        <Link to="/subjects" style={{ color: "white", textDecoration: "none" }}>
                            Môn học
                        </Link>
                    </li>
                    <li>
                        {/* Dùng link test với classId = 11 */}
                        <Link to="/teacher/class/11" style={{ color: "white", textDecoration: "none" }}>
                            Nhập điểm (test lớp 11)
                        </Link>
                    </li>
                    <li>
                        <Link to="/admin/approve" style={{ color: "white", textDecoration: "none" }}>
                            Duyệt điểm
                        </Link>
                    </li>

                </ul>
            </nav>

            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/subjects" element={<Subjects />} />
                <Route path="/teacher/class/:id" element={<ScoreInputPageWrapper />} />
                <Route path="/admin/approve" element={<ApprovePage />} />

            </Routes>
        </Router>
    );
}

export default App;
