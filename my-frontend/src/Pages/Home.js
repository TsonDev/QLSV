import React from "react";
import { Link } from "react-router-dom";

function Home() {
    return (
        <div style={{ textAlign: "center", marginTop: "50px" }}>
            <h1>🏫 Hệ thống quản lý môn học</h1>
            <p>Chào mừng bạn đến với trang chủ!</p>
            <Link
                to="/subjects"
                style={{
                    display: "inline-block",
                    marginTop: "20px",
                    backgroundColor: "#007bff",
                    color: "white",
                    padding: "10px 20px",
                    borderRadius: "6px",
                    textDecoration: "none",
                }}
            >
                Xem danh sách môn học →
            </Link>
        </div>
    );
}

export default Home;
