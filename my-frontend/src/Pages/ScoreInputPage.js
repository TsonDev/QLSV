import React, { useEffect, useState } from "react";

export default function ScoreInputPage({ classId }) {
    const [scores, setScores] = useState([]);
    const [loading, setLoading] = useState(true);
    const API_BASE = "https://localhost:7287/api/StudentSubjects";

    const loadScores = async () => {
        setLoading(true);
        try {
            const res = await fetch(`${API_BASE}/class/${classId}`);
            if (!res.ok) {
                const txt = await res.text();
                throw new Error(`Status ${res.status}: ${txt}`);
            }
            const data = await res.json();
            setScores(data);
        } catch (err) {
            console.error("Load scores error:", err);
            alert("Lỗi khi tải dữ liệu: " + err.message);
            setScores([]);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (classId) loadScores();
    }, [classId]);

    const updateLocal = (studentId, field, value) => {
        setScores(prev =>
            prev.map(item =>
                item.studentId === studentId ? { ...item, [field]: value } : item
            )
        );
    };

    const saveScore = async (item) => {
        const body = {
            point1: item.point1 !== undefined ? Number(item.point1) : null,
            point2: item.point2 !== undefined ? Number(item.point2) : null,
            point3: item.point3 !== undefined ? Number(item.point3) : null,
        };

        try {
            const res = await fetch(
                `${API_BASE}/student-subject/${item.studentId}/${item.subjectId}/${item.semesterId}`,
                {
                    method: "PUT",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(body),
                }
            );

            if (!res.ok) {
                const txt = await res.text();
                throw new Error(`Status ${res.status}: ${txt}`);
            }

            const result = await res.json();
            alert(result.message || "Đã lưu");
            loadScores();
        } catch (err) {
            console.error("Save error:", err);
            alert("Lỗi khi lưu: " + err.message);
        }
    };

    const submitClass = async () => {
        if (!window.confirm("Bạn có chắc muốn gửi điểm lớp này?")) return;
        try {
            const res = await fetch(`${API_BASE}/class/${classId}/submit`, { method: "PUT" });
            if (!res.ok) {
                const txt = await res.text();
                throw new Error(`Status ${res.status}: ${txt}`);
            }
            const result = await res.json();
            alert(result.message || "Đã gửi lớp");
            loadScores();
        } catch (err) {
            console.error("Submit error:", err);
            alert("Lỗi khi gửi: " + err.message);
        }
    };

    if (loading) return <div style={{ padding: 20 }}>Đang tải...</div>;
    if (!scores || scores.length === 0) return <div style={{ padding: 20 }}>Không có dữ liệu cho lớp {classId}</div>;

    return (
        <div style={{ padding: "20px" }}>
            <h2>Nhập điểm — Lớp {classId}</h2>

            <div style={{ marginBottom: 12 }}>
                <button onClick={submitClass} style={{ background: "green", color: "white", padding: "8px 12px", border: "none", borderRadius: 4 }}>
                    Gửi điểm lớp
                </button>
                <button onClick={loadScores} style={{ marginLeft: 8, padding: "8px 12px" }}>
                    Làm mới
                </button>
            </div>

            <table border="1" cellPadding="6" style={{ width: "100%", borderCollapse: "collapse" }}>
                <thead>
                    <tr>
                        <th>Mã SV</th>
                        <th>Tên</th>
                        <th>Điểm 1</th>
                        <th>Điểm 2</th>
                        <th>Điểm 3</th>
                        <th>Tổng</th>
                        <th>Trạng thái</th>
                        <th>Hành động</th>
                    </tr>
                </thead>
                <tbody>
                    {scores.map((item, idx) => (
                        <tr key={idx}>
                            <td>{item.studentId}</td>
                            <td>{item.studentName}</td>

                            <td>
                                <input
                                    type="number"
                                    step="0.1"
                                    value={item.point1 ?? ""}
                                    onChange={(e) => updateLocal(item.studentId, "point1", e.target.value)}
                                    style={{ width: 80 }}
                                />
                            </td>

                            <td>
                                <input
                                    type="number"
                                    step="0.1"
                                    value={item.point2 ?? ""}
                                    onChange={(e) => updateLocal(item.studentId, "point2", e.target.value)}
                                    style={{ width: 80 }}
                                />
                            </td>

                            <td>
                                <input
                                    type="number"
                                    step="0.1"
                                    value={item.point3 ?? ""}
                                    onChange={(e) => updateLocal(item.studentId, "point3", e.target.value)}
                                    style={{ width: 80 }}
                                />
                            </td>

                            <td>{item.pointTotal ?? "-"}</td>
                            <td>{item.isApproved === 1 ? "Đã duyệt" : "Chưa duyệt"}</td>

                            <td>
                                <button onClick={() => saveScore(item)} style={{ padding: "6px 10px" }}>
                                    Lưu
                                </button>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}
