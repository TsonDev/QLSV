import { useEffect, useState } from "react";

export default function AccountsPage() {
    const [accounts, setAccounts] = useState([]);
    const [tickets, setTickets] = useState([]);
    const [loading, setLoading] = useState(true);
    const [filter, setFilter] = useState("all"); // all | active | ticket

    const token = localStorage.getItem("token");

    // ======================
    // Load danh sách tài khoản
    // ======================
    const loadAccounts = async () => {
        const res = await fetch("https://localhost:7287/api/Accounts", {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!res.ok) throw new Error("Không lấy được danh sách tài khoản");
        return res.json();
    };

    // ======================
    // Load danh sách yêu cầu reset
    // ======================
    const loadTickets = async () => {
        const res = await fetch("https://localhost:7287/api/ResetTickets", {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!res.ok) throw new Error("Không lấy được danh sách yêu cầu reset");
        return res.json();
    };

    useEffect(() => {
        const loadAll = async () => {
            try {
                setLoading(true);
                const accData = await loadAccounts();
                const ticketData = await loadTickets();

                setAccounts(accData);
                setTickets(ticketData);
            } catch (err) {
                console.error("Lỗi:", err);
            } finally {
                setLoading(false);
            }
        };

        loadAll();
    }, []);

    // ======================
    // Duyệt yêu cầu reset
    // ======================
    const approveReset = async (id) => {
        if (!window.confirm("Bạn có chắc muốn reset mật khẩu cho tài khoản này?")) return;

        const res = await fetch(
            `https://localhost:7287/api/Accounts/reset-password/by-ticket/${id}`,
            {
                method: "PUT",
                headers: { "Authorization": `Bearer ${token}` }
            }
        );

        const data = await res.json();
        alert(`${data.message}\n\nUsername: ${data.username}\nMật khẩu mới: ${data.newPassword}`);
        window.location.reload();
    };

    // ======================
    // Từ chối yêu cầu reset
    // ======================
    const rejectReset = async (id) => {
        if (!window.confirm("Từ chối yêu cầu này?")) return;

        const res = await fetch(
            `https://localhost:7287/api/Accounts/reset-password/reject/${id}`,
            {
                method: "PUT",
                headers: { "Authorization": `Bearer ${token}` }
            }
        );

        const data = await res.json();
        alert(`${data.message}`);
        window.location.reload();
    };

    if (loading) return <h3>Đang tải dữ liệu...</h3>;

    return (
        <div style={{ padding: 20 }}>
            <h2>Quản lý tài khoản & reset mật khẩu</h2>

            {/* ===================== Filter buttons ===================== */}
            <div style={{ marginBottom: 20 }}>
                <button onClick={() => setFilter("all")} style={btn(filter === "all")}>
                    Tất cả
                </button>
                <button onClick={() => setFilter("active")} style={btn(filter === "active")}>
                    Hoạt động / Khóa
                </button>
                <button onClick={() => setFilter("ticket")} style={btn(filter === "ticket")}>
                    Yêu cầu reset
                </button>
            </div>

            {/* ===================== TABLE TÀI KHOẢN ===================== */}
            {filter !== "ticket" && (
                <table style={table}>
                    <thead>
                        <tr style={headRow}>
                            <th style={cell}>Mã</th>
                            <th style={cell}>Username</th>
                            <th style={cell}>Role</th>
                            <th style={cell}>Trạng thái</th>
                        </tr>
                    </thead>

                    <tbody>
                        {accounts.map(acc => (
                            <tr key={acc.accId}>
                                <td style={cell}>{acc.accId}</td>
                                <td style={cell}>{acc.username}</td>
                                <td style={cell}>{acc.role}</td>
                                <td style={cell}>
                                    {acc.status.trim() === "Active"
                                        ? <span style={{ color: "green" }}>Hoạt động</span>
                                        : <span style={{ color: "red" }}>Đã khóa</span>}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            {/* ===================== TABLE YÊU CẦU RESET ===================== */}
            {filter === "ticket" && (
                <table style={table}>
                    <thead>
                        <tr style={headRow}>
                            <th style={cell}>ID</th>
                            <th style={cell}>Username</th>
                            <th style={cell}>Thời gian</th>
                            <th style={cell}>Trạng thái</th>
                            <th style={cell}>Hành động</th>
                        </tr>
                    </thead>

                    <tbody>
                        {tickets.length === 0 && (
                            <tr>
                                <td colSpan="5" style={cell}>Không có yêu cầu reset.</td>
                            </tr>
                        )}

                        {tickets.map(t => (
                            <tr key={t.id}>
                                <td style={cell}>{t.id}</td>
                                <td style={cell}>{t.username}</td>
                                <td style={cell}>{new Date(t.requestTime).toLocaleString("vi-VN")}</td>
                                <td style={cell}>
                                    {t.status === "Pending"
                                        ? <span style={{ color: "orange" }}>Đang chờ</span>
                                        : t.status === "Approved"
                                            ? <span style={{ color: "green" }}>Đã reset</span>
                                            : <span style={{ color: "red" }}>Đã từ chối</span>}
                                </td>
                                <td style={cell}>
                                    {t.status === "Pending" && (
                                        <>
                                            <button onClick={() => approveReset(t.id)} style={btnGreen}>
                                                Duyệt
                                            </button>
                                            <button onClick={() => rejectReset(t.id)} style={btnRed}>
                                                Từ chối
                                            </button>
                                        </>
                                    )}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}
        </div>
    );
}

/* ====================== CSS ====================== */

const table = {
    width: "100%",
    borderCollapse: "collapse",
    background: "#fff",
    marginTop: 10
};

const headRow = {
    background: "#f0f0f0",
    borderBottom: "2px solid #ccc"
};

const cell = {
    padding: "10px",
    borderBottom: "1px solid #ddd",
    textAlign: "center"
};

const btn = (active) => ({
    padding: "8px 14px",
    marginRight: "10px",
    borderRadius: "5px",
    cursor: "pointer",
    background: active ? "#3498db" : "#ccc",
    color: "white",
    border: "none"
});

const btnGreen = {
    padding: "6px 12px",
    background: "green",
    color: "white",
    border: "none",
    borderRadius: "5px",
    marginRight: "5px",
    cursor: "pointer"
};

const btnRed = {
    padding: "6px 12px",
    background: "red",
    color: "white",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer"
};
