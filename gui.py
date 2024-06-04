import pygame

class GUI:
    def __init__(self):
        pygame.init()
        self.screen = pygame.display.set_mode((640, 640))
        pygame.display.set_caption("Chess")
        self.clock = pygame.time.Clock()

    def run(self):
        running = True

        while running:
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    running = False

                if event.type == pygame.KEYDOWN and event.key == pygame.K_ESCAPE:
                    running = False

            for i in range(8):
                for j in range(8):
                    color = (50, 168, 82) if (i + j) % 2 else (191, 246, 195)
                        
                    pygame.draw.rect(self.screen, color, (i * 80, j * 80, 80, 80))

            pygame.display.flip()
            
            self.clock.tick(60)

        pygame.quit()

GUI().run()