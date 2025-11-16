import React from "react";
import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import Home from "./Pages/Home";
import Subjects from "./Pages/Subjects";

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
                </ul>
            </nav>

            <Routes>
                <Route path="/" element={<Home />} />
                <Route path="/subjects" element={<Subjects />} />
            </Routes>
        </Router>
    );
}

export default App;
