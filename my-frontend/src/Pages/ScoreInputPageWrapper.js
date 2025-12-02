import React from "react";
import { useParams } from "react-router-dom";
import ScoreInputPage from "./ScoreInputPage";

export default function ScoreInputPageWrapper() {
    const { id } = useParams();
    return <ScoreInputPage classId={id} />;
}
