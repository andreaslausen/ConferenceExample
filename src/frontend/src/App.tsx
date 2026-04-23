import { Routes, Route } from "react-router-dom";
import ConferenceListPage from "./pages/ConferenceListPage";
import ConferenceProgramPage from "./pages/ConferenceProgramPage";
import TalkDetailPage from "./pages/TalkDetailPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import SpeakerProfilePage from "./pages/SpeakerProfilePage";
import MyTalksPage from "./pages/MyTalksPage";
import SubmitTalkPage from "./pages/SubmitTalkPage";
import EditTalkPage from "./pages/EditTalkPage";
import OrganizerConferenceListPage from "./pages/OrganizerConferenceListPage";
import OrganizerNewConferencePage from "./pages/OrganizerNewConferencePage";
import OrganizerConferenceDetailPage from "./pages/OrganizerConferenceDetailPage";
import OrganizerTalkTypesPage from "./pages/OrganizerTalkTypesPage";
import OrganizerRoomsPage from "./pages/OrganizerRoomsPage";
import OrganizerProposalsPage from "./pages/OrganizerProposalsPage";
import OrganizerSchedulePage from "./pages/OrganizerSchedulePage";
import NotFoundPage from "./pages/NotFoundPage";

export default function App() {
  return (
    <Routes>
      {/* Public (Attendee) */}
      <Route path="/" element={<ConferenceListPage />} />
      <Route path="/conferences/:id" element={<ConferenceProgramPage />} />
      <Route path="/talks/:id" element={<TalkDetailPage />} />

      {/* Auth */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      {/* Speaker */}
      <Route path="/profile" element={<SpeakerProfilePage />} />
      <Route path="/my-talks" element={<MyTalksPage />} />
      <Route path="/my-talks/submit" element={<SubmitTalkPage />} />
      <Route path="/my-talks/:id/edit" element={<EditTalkPage />} />

      {/* Organizer */}
      <Route path="/organizer/conferences" element={<OrganizerConferenceListPage />} />
      <Route path="/organizer/conferences/new" element={<OrganizerNewConferencePage />} />
      <Route path="/organizer/conferences/:id" element={<OrganizerConferenceDetailPage />} />
      <Route path="/organizer/conferences/:id/talk-types" element={<OrganizerTalkTypesPage />} />
      <Route path="/organizer/conferences/:id/rooms" element={<OrganizerRoomsPage />} />
      <Route path="/organizer/conferences/:id/proposals" element={<OrganizerProposalsPage />} />
      <Route path="/organizer/conferences/:id/schedule" element={<OrganizerSchedulePage />} />

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
