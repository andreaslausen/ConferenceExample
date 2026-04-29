import { Routes, Route } from "react-router-dom";
import { Header } from "./shared/components/Header";
import { ProtectedRoute } from "./shared/components/ProtectedRoute";
import { RoleGuard } from "./shared/components/RoleGuard";
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
import EditConferencePage from "./pages/EditConferencePage";
import OrganizerTalkTypesPage from "./pages/OrganizerTalkTypesPage";
import OrganizerRoomsPage from "./pages/OrganizerRoomsPage";
import OrganizerProposalsPage from "./pages/OrganizerProposalsPage";
import OrganizerSchedulePage from "./pages/OrganizerSchedulePage";
import ForbiddenPage from "./pages/ForbiddenPage";
import NotFoundPage from "./pages/NotFoundPage";

export default function App() {
  return (
    <div className="min-h-screen">
      <Header />
      <main>
        <Routes>
          {/* Public (Attendee) */}
          <Route path="/" element={<ConferenceListPage />} />
          <Route path="/conferences/:id" element={<ConferenceProgramPage />} />
          <Route path="/talks/:id" element={<TalkDetailPage />} />

          {/* Auth */}
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />

          {/* Speaker */}
          <Route
            path="/profile"
            element={
              <ProtectedRoute>
                <RoleGuard role="Speaker">
                  <SpeakerProfilePage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/my-talks"
            element={
              <ProtectedRoute>
                <RoleGuard role="Speaker">
                  <MyTalksPage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/my-talks/submit"
            element={
              <ProtectedRoute>
                <RoleGuard role="Speaker">
                  <SubmitTalkPage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/my-talks/:id/edit"
            element={
              <ProtectedRoute>
                <RoleGuard role="Speaker">
                  <EditTalkPage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />

          {/* Organizer */}
          <Route
            path="/organizer/conferences"
            element={
              <ProtectedRoute>
                <RoleGuard role="Organizer">
                  <OrganizerConferenceListPage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/organizer/conferences/new"
            element={
              <ProtectedRoute>
                <RoleGuard role="Organizer">
                  <OrganizerNewConferencePage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/organizer/conferences/:id"
            element={
              <ProtectedRoute>
                <RoleGuard role="Organizer">
                  <OrganizerConferenceDetailPage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/organizer/conferences/:id/edit"
            element={
              <ProtectedRoute>
                <RoleGuard role="Organizer">
                  <EditConferencePage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/organizer/conferences/:id/talk-types"
            element={
              <ProtectedRoute>
                <RoleGuard role="Organizer">
                  <OrganizerTalkTypesPage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/organizer/conferences/:id/rooms"
            element={
              <ProtectedRoute>
                <RoleGuard role="Organizer">
                  <OrganizerRoomsPage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/organizer/conferences/:id/proposals"
            element={
              <ProtectedRoute>
                <RoleGuard role="Organizer">
                  <OrganizerProposalsPage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/organizer/conferences/:id/schedule"
            element={
              <ProtectedRoute>
                <RoleGuard role="Organizer">
                  <OrganizerSchedulePage />
                </RoleGuard>
              </ProtectedRoute>
            }
          />

          <Route path="/forbidden" element={<ForbiddenPage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </main>
    </div>
  );
}
