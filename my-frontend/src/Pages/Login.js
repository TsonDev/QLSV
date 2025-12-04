import React, { useState } from "react";

const Login = () => {
    const [username, setUsername] = useState("");
    const [password, setPassword] = useState("");
    const [msg, setMsg] = useState("");

    const handleLogin = async (e) => {
        e.preventDefault();

        try {
            const res = await fetch("https://localhost:7287/api/Accounts/login", {

                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    username: username,
                    password: password
                })
            });

            if (!res.ok) {
                const errorText = await res.text();
                setMsg(errorText);
                return;
            }

            const data = await res.json();
            localStorage.setItem("token", data.token);

            setMsg("Đăng nhập thành công!");

            // 👉 CHUYỂN SANG TRANG HOME
            window.location.href = "/home";

        } catch (err) {
            console.error(err);
            setMsg("Lỗi kết nối server!");
        }
    };

    return (
        <div style={styles.container}>
            <form style={styles.form} onSubmit={handleLogin}>
                <h2 style={{ textAlign: "center" }}>Đăng nhập</h2>

                <p style={{ color: "red", textAlign: "center" }}>{msg}</p>

                <input
                    type="text"
                    placeholder="Username"
                    style={styles.input}
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                />

                <input
                    type="password"
                    placeholder="Password"
                    style={styles.input}
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                />

                <button type="submit" style={styles.button}>Đăng nhập</button>
            </form>
        </div>
    );
};

const styles = {
    container: {
        display: "flex",
        height: "100vh",
        justifyContent: "center",
        alignItems: "center",
        background: "#f2f2f2"
    },
    form: {
        width: "350px",
        padding: "25px",
        background: "#fff",
        borderRadius: "10px",
        boxShadow: "0 5px 15px rgba(0,0,0,0.1)"
    },
    input: {
        width: "100%",
        padding: "10px",
        margin: "10px 0",
        borderRadius: "5px",
        border: "1px solid #ccc"
    },
    button: {
        width: "100%",
        padding: "10px",
        background: "#007bff",
        color: "#fff",
        border: "none",
        borderRadius: "5px",
        cursor: "pointer"
    }
};

export default Login;
