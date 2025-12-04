import React from "react";
import { BrowserRouter as Router, Routes, Route, Link, Navigate } from "react-router-dom";
import Home from "./Pages/Home";
import Subjects from "./Pages/Subjects";
import ScoreInputPageWrapper from "./Pages/ScoreInputPageWrapper";
import ApprovePage from "./Pages/ApprovePage";
import Login from "./Pages/Login";

function App() {
    const token = localStorage.getItem("token");

    return (
        <Router>

            {/* =============== MENU =============== */}
            {token && (
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
                            <Link to="/home" style={{ color: "white" }}>Trang chủ</Link>
                        </li>
                        <li>
                            <Link to="/subjects" style={{ color: "white" }}>Môn học</Link>
                        </li>
                        <li>
                            <Link to="/teacher/class/11" style={{ color: "white" }}>Nhập điểm</Link>
                        </li>
                        <li>
                            <Link to="/admin/approve" style={{ color: "white" }}>Duyệt điểm</Link>
                        </li>
                        <li>
                            <button
                                onClick={() => {
                                    localStorage.removeItem("token");
                                    window.location.href = "/login";
                                }}
                                style={{
                                    background: "red",
                                    color: "white",
                                    border: "none",
                                    padding: "5px 10px",
                                    borderRadius: "5px"
                                }}
                            >
                                Đăng xuất
                            </button>
                        </li>
                    </ul>
                </nav>
            )}

            {/* =============== ROUTES =============== */}
            <Routes>
                {/* Trang login */}
                <Route path="/login" element={<Login />} />

                {/* Khi vào "/", chuyển qua login luôn */}
                <Route path="/" element={<Navigate to="/login" />} />

                {/* Sau khi login → vào trang home */}
                <Route
                    path="/home"
                    element={token ? <Home /> : <Navigate to="/login" />}
                />

                <Route
                    path="/subjects"
                    element={token ? <Subjects /> : <Navigate to="/login" />}
                />

                <Route
                    path="/teacher/class/:id"
                    element={token ? <ScoreInputPageWrapper /> : <Navigate to="/login" />}
                />

                <Route
                    path="/admin/approve"
                    element={token ? <ApprovePage /> : <Navigate to="/login" />}
                />
            </Routes>
        </Router>
    );
}

export default App;
