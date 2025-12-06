import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

const Login = () => {
    const navigate = useNavigate();

    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [msg, setMsg] = useState("");

    // Popup quên mật khẩu
    const [showForgot, setShowForgot] = useState(false);
    const [forgotUser, setForgotUser] = useState("");
    const [forgotMsg, setForgotMsg] = useState("");

    // ============================
    // Xử lý đăng nhập
    // ============================
    const handleLogin = async (e) => {
        e.preventDefault();

        try {
            const res = await fetch("https://localhost:7287/api/Accounts/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username, password })
            });

            const data = await res.json();

            if (!res.ok) {
                setMsg(data.message || "Sai tài khoản hoặc mật khẩu!");
                return;
            }

            localStorage.setItem("token", data.token);
            localStorage.setItem("role", data.role);

            // Điều hướng theo role
            switch (data.role) {
                case "Admin": navigate("/admin/Home"); break;
                case "Teacher": navigate("/Teacher/Home"); break;
                case "Advisor": navigate("/advisor/Home"); break;
                default: navigate("/home");
            }

        } catch {
            setMsg("Không thể kết nối server!");
        }
    };

    // ============================
    // Xử lý yêu cầu reset mật khẩu
    // ============================
    const handleForgot = async () => {
        if (!forgotUser.trim()) {
            setForgotMsg("⚠ Vui lòng nhập username!");
            return;
        }

        setForgotMsg("Đang gửi yêu cầu...");

        try {
            const res = await fetch("https://localhost:7287/api/Accounts/request-reset", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ username: forgotUser })
            });

            const text = await res.text();

            if (!res.ok) {
                setForgotMsg("❌ " + text);
                return;
            }

            setForgotMsg("✅ Yêu cầu đã được gửi! Admin sẽ xử lý.");
        } catch {
            setForgotMsg("❌ Không thể gửi yêu cầu!");
        }
    };

    // Đóng popup → reset dữ liệu
    const closeForgot = () => {
        setShowForgot(false);
        setForgotUser("");
        setForgotMsg("");
    };

    return (
        <div style={{
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            height: "100vh",
            background: "#f5f6fa"
        }}>
            <form onSubmit={handleLogin} style={{
                width: "320px",
                padding: "20px",
                background: "#fff",
                borderRadius: "8px",
                boxShadow: "0 0 10px rgba(0,0,0,0.1)"
            }}>
                <h3 style={{ textAlign: "center" }}>Đăng nhập</h3>

                <p style={{ color: "red", textAlign: "center" }}>{msg}</p>

                <input
                    type="text"
                    placeholder="Username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    style={inputStyle}
                />

                <input
                    type="password"
                    placeholder="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    style={inputStyle}
                />

                <button type="submit" style={btnLogin}>Đăng nhập</button>

                <p
                    onClick={() => setShowForgot(true)}
                    style={{
                        color: "blue",
                        cursor: "pointer",
                        textAlign: "center",
                        marginTop: "10px"
                    }}
                >
                    Quên mật khẩu?
                </p>
            </form>

            {/* ======================= POPUP QUÊN MẬT KHẨU ======================= */}
            {showForgot && (
                <div style={popupOverlay}>
                    <div style={popupBox}>
                        <h3>Yêu cầu đặt lại mật khẩu</h3>

                        <input
                            type="text"
                            placeholder="Nhập Username"
                            value={forgotUser}
                            onChange={(e) => setForgotUser(e.target.value)}
                            style={inputStyle}
                        />

                        <p style={{ color: forgotMsg.startsWith("✅") ? "green" : "red" }}>
                            {forgotMsg}
                        </p>

                        <div style={{ marginTop: "10px", textAlign: "right" }}>
                            <button onClick={handleForgot} style={btnBlue}>
                                Gửi yêu cầu
                            </button>
                            <button onClick={closeForgot} style={btnCancel}>
                                Đóng
                            </button>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

/* ========================== CSS ============================ */

const inputStyle = {
    width: "100%",
    padding: "10px",
    margin: "8px 0",
    borderRadius: "5px",
    border: "1px solid #ccc"
};

const btnLogin = {
    width: "100%",
    padding: "10px",
    background: "#3498db",
    color: "white",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer",
    fontWeight: "bold",
    marginTop: "10px"
};

const btnBlue = {
    padding: "8px 14px",
    background: "#2980b9",
    color: "white",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer",
    marginRight: "8px"
};

const btnCancel = {
    padding: "8px 14px",
    background: "#7f8c8d",
    color: "white",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer"
};

const popupOverlay = {
    position: "fixed",
    top: 0, left: 0, right: 0, bottom: 0,
    background: "rgba(0,0,0,0.5)",
    display: "flex",
    justifyContent: "center",
    alignItems: "center"
};

const popupBox = {
    width: "350px",
    background: "white",
    padding: "20px",
    borderRadius: "8px",
    boxShadow: "0 0 10px rgba(0,0,0,0.3)"
};

export default Login;
